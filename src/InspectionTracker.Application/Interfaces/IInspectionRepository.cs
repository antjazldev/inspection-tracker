using InspectionTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InspectionTracker.Application.Interfaces
{
    public interface IInspectionRepository
    {
        Task AddAsync(InspectionRecord record, CancellationToken ct = default);
        Task<InspectionRecord?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<InspectionRecord>> GetAllAsync(CancellationToken ct = default);
        Task UpdateAsync(InspectionRecord record, CancellationToken ct = default);
        Task DeleteAsync(InspectionRecord record, CancellationToken ct = default);
    }
}
