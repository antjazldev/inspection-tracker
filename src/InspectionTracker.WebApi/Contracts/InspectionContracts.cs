using InspectionTracker.Domain.Enums;

namespace InspectionTracker.WebApi.Contracts;

public record InspectionRequest(string AssetName, InspectionStatus Status, string? Notes, DateTime InspectionDate);
public record InspectionResponse(Guid Id, string AssetName, InspectionStatus Status, string? Notes, DateTime InspectionDate, Guid CreatedByUserId, string CreatedByName);
