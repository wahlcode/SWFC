namespace SWFC.Application.M800_Security.M805_AuditCompliance.Entries;

public sealed record AuditEntryListItem(
    Guid Id,
    DateTime TimestampUtc,
    string ActorUserId,
    string ActorDisplayName,
    string? TargetUserId,
    string Action,
    string Module,
    string ObjectType,
    string ObjectId,
    string? OldValues,
    string? NewValues,
    string? ClientIp,
    string? ClientInfo,
    string? ApprovedByUserId,
    string? Reason);
