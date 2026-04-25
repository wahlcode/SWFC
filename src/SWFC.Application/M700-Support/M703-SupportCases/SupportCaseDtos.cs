namespace SWFC.Application.M700_Support.M703_SupportCases;

public sealed record SupportCaseListItem(
    Guid Id,
    string UserRequest,
    string ProblemDescription,
    Guid? TriggeredBugId,
    Guid? TriggeredIncidentId,
    DateTime CreatedAtUtc,
    string CreatedBy,
    DateTime? LastModifiedAtUtc,
    string? LastModifiedBy);
