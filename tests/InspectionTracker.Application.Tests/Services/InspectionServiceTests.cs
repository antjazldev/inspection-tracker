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
        await act.Should().ThrowAsync<ValidationException>()
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
        await act.Should().ThrowAsync<ValidationException>()
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
        await act.Should().ThrowAsync<ValidationException>()
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

    private void VerifyAddCalled(Times times) =>
        _repoMock.Verify(
            r => r.AddAsync(It.IsAny<InspectionRecord>(), It.IsAny<CancellationToken>()),
            times);
}