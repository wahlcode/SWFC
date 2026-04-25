using SWFC.Domain.M700_Support.M702_ChangeRequests;

namespace SWFC.Application.M700_Support.M702_ChangeRequests;

public sealed record ChangeRequestListItem(
    Guid Id,
    ChangeRequestType Type,
    string Description,
    string? RequirementReference,
    string? RoadmapReference,
    DateTime CreatedAtUtc,
    string CreatedBy,
    DateTime? LastModifiedAtUtc,
    string? LastModifiedBy);
