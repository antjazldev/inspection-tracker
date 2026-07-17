using InspectionTracker.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InspectionTracker.Application.Dtos
{
    public record CreateInspectionDto(
    string AssetName,
    InspectionStatus Status,
    string? Notes,
    DateTime InspectionDate);
}
