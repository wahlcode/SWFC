namespace SWFC.Application.M700_Support.M706_SLA_Service_Levels;

public sealed record ServiceLevelListItem(
    Guid Id,
    string Priority,
    TimeSpan ResponseTime,
    TimeSpan ProcessingTime,
    bool UseForSupport,
    bool UseForIncidentManagement,
    DateTime CreatedAtUtc,
    string CreatedBy,
    DateTime? LastModifiedAtUtc,
    string? LastModifiedBy);
