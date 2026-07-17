using InspectionTracker.Application.Interfaces;
using InspectionTracker.Domain.Entities;
using InspectionTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InspectionTracker.Infrastructure.Repositories
{
    public class InspectionRepository(AppDbContext db) : IInspectionRepository
    {
        public async Task AddAsync(InspectionRecord record, CancellationToken ct = default)
        {
            db.Inspections.Add(record);
            await db.SaveChangesAsync(ct);
        }

        public Task<InspectionRecord?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => db.Inspections.FirstOrDefaultAsync(i => i.Id == id, ct);

        public async Task<IReadOnlyList<InspectionRecord>> GetAllAsync(CancellationToken ct = default)
            => await db.Inspections
                .AsNoTracking()
                .OrderByDescending(i => i.InspectionDate)
                .ToListAsync(ct);

        public async Task UpdateAsync(InspectionRecord record, CancellationToken ct = default)
        {
            db.Inspections.Update(record);
            await db.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(InspectionRecord record, CancellationToken ct = default)
        {
            db.Inspections.Remove(record);
            await db.SaveChangesAsync(ct);
        }
    }
}
