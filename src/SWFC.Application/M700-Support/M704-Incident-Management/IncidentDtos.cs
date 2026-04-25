using SWFC.Domain.M700_Support.M704_Incident_Management;

namespace SWFC.Application.M700_Support.M704_Incident_Management;

public sealed record IncidentListItem(
    Guid Id,
    IncidentCategory Category,
    string Description,
    string Escalation,
    string ReactionControl,
    string? NotificationReference,
    string? RuntimeReference,
    DateTime CreatedAtUtc,
    string CreatedBy,
    DateTime? LastModifiedAtUtc,
    string? LastModifiedBy);
