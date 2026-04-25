using SWFC.Domain.M700_Support.M701_BugTracking;

namespace SWFC.Application.M700_Support.M701_BugTracking;

public sealed record BugListItem(
    Guid Id,
    string Description,
    string Reproducibility,
    BugStatus Status,
    DateTime CreatedAtUtc,
    string CreatedBy,
    DateTime? LastModifiedAtUtc,
    string? LastModifiedBy);
