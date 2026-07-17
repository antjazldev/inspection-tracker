using FluentAssertions;
using InspectionTracker.Application.Dtos;
using InspectionTracker.Application.Exceptions;
using InspectionTracker.Application.Interfaces;
using InspectionTracker.Application.Services;
using InspectionTracker.Domain.Entities;
using InspectionTracker.Domain.Enums;
using Moq;
using System.ComponentModel.DataAnnotations;

namespace InspectionTracker.Application.Tests.Services;

public class InspectionServiceTests
{
    private const string DefaultAssetName = "Chainsaw 042";

    private readonly Mock<IInspectionRepository> _repoMock = new();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly InspectionService _sut;

    public InspectionServiceTests()
    {
        _sut = new InspectionService(_repoMock.Object);
    }

    private static CreateInspectionDto ValidDto() => new(
        AssetName: DefaultAssetName,
        Status: InspectionStatus.Passed,
        Notes: null,
        InspectionDate: DateTime.UtcNow.AddHours(-1));

    [Fact]
    public async Task CreateAsync_WithValidData_SavesAndReturnsRecord()
    {
        // Arrange
        var dto = ValidDto();

        // Act
        var result = await _sut.CreateAsync(dto, _userId);

        // Assert
        result.Id.Should().NotBeEmpty();
        result.AssetName.Should().Be(DefaultAssetName);
        result.CreatedByUserId.Should().Be(_userId);
        VerifyAddCalled(Times.Once());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_WithMissingAssetName_ThrowsValidation(string assetName)
    {
        // Arrange
        var dto = ValidDto() with { AssetName = assetName };

        // Act
        Func<Task> act = () => _sut.CreateAsync(dto, _userId);

        // Assert
        await act.Should().ThrowAsync<BusinessValidationException>()
            .WithMessage("*asset name*");
        VerifyAddCalled(Times.Never());
    }

    [Fact]
    public async Task CreateAsync_WithFutureDate_ThrowsValidation()
    {
        // Arrange
        var dto = ValidDto() with { InspectionDate = DateTime.UtcNow.AddDays(1) };

        // Act
        Func<Task> act = () => _sut.CreateAsync(dto, _userId);

        // Assert
        await act.Should().ThrowAsync<BusinessValidationException>()
            .WithMessage("*future*");
        VerifyAddCalled(Times.Never());
    }

    [Fact]
    public async Task CreateAsync_FailedWithoutNotes_ThrowsValidation()
    {
        // Arrange
        var dto = ValidDto() with { Status = InspectionStatus.Failed, Notes = null };

        // Act
        Func<Task> act = () => _sut.CreateAsync(dto, _userId);

        // Assert
        await act.Should().ThrowAsync<BusinessValidationException>()
            .WithMessage("*notes*");
        VerifyAddCalled(Times.Never());
    }

    [Fact]
    public async Task CreateAsync_FailedWithNotes_Succeeds()
    {
        // Arrange
        var dto = ValidDto() with
        {
            Status = InspectionStatus.Failed,
            Notes = "Chain tension out of spec"
        };

        // Act
        var result = await _sut.CreateAsync(dto, _userId);

        // Assert
        result.Status.Should().Be(InspectionStatus.Failed);
        result.Notes.Should().Be("Chain tension out of spec");
        VerifyAddCalled(Times.Once());
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ThrowsNotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((InspectionRecord?)null);

        Func<Task> act = () => _sut.GetByIdAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_WhenUserIsNotOwner_ThrowsForbidden()
    {
        var record = ExistingRecord(ownerId: Guid.NewGuid());
        SetupRepoReturns(record);

        Func<Task> act = () => _sut.UpdateAsync(record.Id, ValidDto(), _userId);

        await act.Should().ThrowAsync<ForbiddenException>();
        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<InspectionRecord>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenOwner_UpdatesFields()
    {
        var record = ExistingRecord(ownerId: _userId);
        SetupRepoReturns(record);
        var dto = ValidDto() with { AssetName = "Mower 007", Status = InspectionStatus.Failed, Notes = "Blade damage" };

        var result = await _sut.UpdateAsync(record.Id, dto, _userId);

        result.AssetName.Should().Be("Mower 007");
        result.Status.Should().Be(InspectionStatus.Failed);
        _repoMock.Verify(r => r.UpdateAsync(record, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidData_ThrowsValidation()
    {
        var record = ExistingRecord(ownerId: _userId);
        SetupRepoReturns(record);
        var dto = ValidDto() with { AssetName = "" };

        Func<Task> act = () => _sut.UpdateAsync(record.Id, dto, _userId);

        await act.Should().ThrowAsync<BusinessValidationException>();
    }

    [Fact]
    public async Task DeleteAsync_WhenUserIsNotOwner_ThrowsForbidden()
    {
        var record = ExistingRecord(ownerId: Guid.NewGuid());
        SetupRepoReturns(record);

        Func<Task> act = () => _sut.DeleteAsync(record.Id, _userId);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task DeleteAsync_WhenOwner_Deletes()
    {
        var record = ExistingRecord(ownerId: _userId);
        SetupRepoReturns(record);

        await _sut.DeleteAsync(record.Id, _userId);

        _repoMock.Verify(r => r.DeleteAsync(record, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllRecords()
    {
        _repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<InspectionRecord> { ExistingRecord(_userId), ExistingRecord(Guid.NewGuid()) });

        var result = await _sut.GetAllAsync();

        result.Should().HaveCount(2);
    }

    private static InspectionRecord ExistingRecord(Guid ownerId) => new()
    {
        Id = Guid.NewGuid(),
        AssetName = DefaultAssetName,
        Status = InspectionStatus.Passed,
        InspectionDate = DateTime.UtcNow.AddDays(-1),
        CreatedByUserId = ownerId
    };

    private void SetupRepoReturns(InspectionRecord record) =>
        _repoMock.Setup(r => r.GetByIdAsync(record.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);

    private void VerifyAddCalled(Times times) =>
        _repoMock.Verify(
            r => r.AddAsync(It.IsAny<InspectionRecord>(), It.IsAny<CancellationToken>()),
            times);
}