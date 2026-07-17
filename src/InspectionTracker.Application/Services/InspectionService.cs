using InspectionTracker.Application.Dtos;
using InspectionTracker.Application.Exceptions;
using InspectionTracker.Application.Interfaces;
using InspectionTracker.Domain.Entities;
using InspectionTracker.Domain.Enums;

namespace InspectionTracker.Application.Services
{
    public class InspectionService(IInspectionRepository repository)
    {
        public async Task<InspectionRecord> CreateAsync(
            CreateInspectionDto dto, Guid userId, CancellationToken ct = default)
        {
            Validate(dto);

            var record = new InspectionRecord
            {
                Id = Guid.NewGuid(),
                AssetName = dto.AssetName.Trim(),
                Status = dto.Status,
                Notes = dto.Notes,
                InspectionDate = dto.InspectionDate,
                CreatedByUserId = userId
            };

            await repository.AddAsync(record, ct);
            return record;
        }

        private static void Validate(CreateInspectionDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.AssetName))
                throw new BusinessValidationException("The asset name is required.");

            if (dto.InspectionDate > DateTime.UtcNow)
                throw new BusinessValidationException("The inspection date cannot be in the future.");

            if (dto.Status == InspectionStatus.Failed && string.IsNullOrWhiteSpace(dto.Notes))
                throw new BusinessValidationException("Failed inspections require notes.");
        }

        public async Task<InspectionRecord> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var record = await repository.GetByIdAsync(id, ct);
            return record ?? throw new NotFoundException($"Inspection {id} was not found.");
        }

        public async Task<IReadOnlyList<InspectionRecord>> GetAllAsync(CancellationToken ct = default)
            => await repository.GetAllAsync(ct);

        public async Task<InspectionRecord> UpdateAsync(
            Guid id, CreateInspectionDto dto, Guid userId, CancellationToken ct = default)
        {
            var record = await GetByIdAsync(id, ct);
            EnsureOwner(record, userId);
            Validate(dto);

            record.AssetName = dto.AssetName.Trim();
            record.Status = dto.Status;
            record.Notes = dto.Notes;
            record.InspectionDate = dto.InspectionDate;

            await repository.UpdateAsync(record, ct);
            return record;
        }

        public async Task DeleteAsync(Guid id, Guid userId, CancellationToken ct = default)
        {
            var record = await GetByIdAsync(id, ct);
            EnsureOwner(record, userId);
            await repository.DeleteAsync(record, ct);
        }

        private static void EnsureOwner(InspectionRecord record, Guid userId)
        {
            if (record.CreatedByUserId != userId)
                throw new ForbiddenException("Only the creator can modify this inspection.");
        }
    }
}
