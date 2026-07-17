using FluentAssertions;
using InspectionTracker.Domain.Entities;
using InspectionTracker.Domain.Enums;
using InspectionTracker.Infrastructure.Persistence;
using InspectionTracker.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace InspectionTracker.Infrastructure.Tests;

public class InspectionRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _db;
    private readonly InspectionRepository _sut;
    private readonly User _owner;

    public InspectionRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();

        _owner = new User
        {
            Id = Guid.NewGuid(),
            Email = "owner@example.com",
            DisplayName = "Owner",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow
        };
        _db.Users.Add(_owner);
        _db.SaveChanges();

        _sut = new InspectionRepository(_db);
    }

    private InspectionRecord NewRecord() => new()
    {
        Id = Guid.NewGuid(),
        AssetName = "Chainsaw 042",
        Status = InspectionStatus.Passed,
        InspectionDate = DateTime.UtcNow.AddDays(-1),
        CreatedByUserId = _owner.Id
    };

    [Fact]
    public async Task AddAsync_PersistsRecord()
    {
        var record = NewRecord();

        await _sut.AddAsync(record);

        var found = await _sut.GetByIdAsync(record.Id);
        found.Should().NotBeNull();
        found!.AssetName.Should().Be("Chainsaw 042");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsNewestFirst()
    {
        var older = NewRecord();
        older.InspectionDate = DateTime.UtcNow.AddDays(-5);
        var newer = NewRecord();

        await _sut.AddAsync(older);
        await _sut.AddAsync(newer);

        var all = await _sut.GetAllAsync();

        all.Should().HaveCount(2);
        all[0].Id.Should().Be(newer.Id);
    }

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        var record = NewRecord();
        await _sut.AddAsync(record);

        record.Status = InspectionStatus.Failed;
        record.Notes = "Blade damage";
        await _sut.UpdateAsync(record);

        var found = await _sut.GetByIdAsync(record.Id);
        found!.Status.Should().Be(InspectionStatus.Failed);
        found.Notes.Should().Be("Blade damage");
    }

    [Fact]
    public async Task DeleteAsync_RemovesRecord()
    {
        var record = NewRecord();
        await _sut.AddAsync(record);

        await _sut.DeleteAsync(record);

        (await _sut.GetByIdAsync(record.Id)).Should().BeNull();
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }
}
