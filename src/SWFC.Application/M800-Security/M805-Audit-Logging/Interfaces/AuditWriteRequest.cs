namespace SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;

public sealed record AuditWriteRequest(
    string ActorUserId,
    string ActorDisplayName,
    string Action,
    string Module,
    string ObjectType,
    string ObjectId,
    DateTime TimestampUtc,
    string? OldValues = null,
    string? NewValues = null,
    string? TargetUserId = null,
    string? ClientIp = null,
    string? ClientInfo = null,
    string? ApprovedByUserId = null,
    string? Reason = null);
