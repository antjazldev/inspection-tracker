using InspectionTracker.Application.Interfaces;
using InspectionTracker.Domain.Entities;
using InspectionTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InspectionTracker.Infrastructure.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
    }
}
