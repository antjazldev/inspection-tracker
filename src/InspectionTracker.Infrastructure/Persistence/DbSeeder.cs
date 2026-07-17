using InspectionTracker.Domain.Entities;
using InspectionTracker.Domain.Enums;

namespace InspectionTracker.Infrastructure.Persistence;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        db.Database.EnsureCreated();

        if (db.Users.Any()) return; // already seeded

        var demoUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "demo@inspectiontracker.com",
            DisplayName = "Demo Supervisor",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Demo1234!"),
            CreatedAt = DateTime.UtcNow
        };
        db.Users.Add(demoUser);

        db.Inspections.AddRange(
            new InspectionRecord
            {
                Id = Guid.NewGuid(),
                AssetName = "Chainsaw 042",
                Status = InspectionStatus.Passed,
                InspectionDate = DateTime.UtcNow.AddDays(-2),
                CreatedByUserId = demoUser.Id
            },
            new InspectionRecord
            {
                Id = Guid.NewGuid(),
                AssetName = "Mower 007",
                Status = InspectionStatus.Failed,
                Notes = "Blade damage found during routine check",
                InspectionDate = DateTime.UtcNow.AddDays(-1),
                CreatedByUserId = demoUser.Id
            },
            new InspectionRecord
            {
                Id = Guid.NewGuid(),
                AssetName = "Chipper 015",
                Status = InspectionStatus.Pending,
                InspectionDate = DateTime.UtcNow.AddHours(-3),
                CreatedByUserId = demoUser.Id
            });

        db.SaveChanges();
    }
}
