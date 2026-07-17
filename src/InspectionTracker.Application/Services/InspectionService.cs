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
    }
}
