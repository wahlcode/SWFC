using SWFC.Domain.M100_System.M101_Foundation.Rules;

namespace SWFC.Domain.M800_Security.M805_AuditCompliance.Entities;

public sealed class AuditLog
{
    private AuditLog()
    {
        Id = Guid.Empty;
        ActorUserId = null!;
        ActorDisplayName = null!;
        Action = null!;
        Module = null!;
        ObjectType = null!;
        ObjectId = null!;
        TimestampUtc = DateTime.UtcNow;
        TargetUserId = null;
        OldValues = null;
        NewValues = null;
        ClientIp = null;
        ClientInfo = null;
        ApprovedByUserId = null;
        Reason = null;
    }

    private AuditLog(
        Guid id,
        string actorUserId,
        string actorDisplayName,
        string action,
        string module,
        string objectType,
        string objectId,
        DateTime timestampUtc,
        string? targetUserId,
        string? oldValues,
        string? newValues,
        string? clientIp,
        string? clientInfo,
        string? approvedByUserId,
        string? reason)
    {
        Id = id;
        ActorUserId = actorUserId;
        ActorDisplayName = actorDisplayName;
        Action = action;
        Module = module;
        ObjectType = objectType;
        ObjectId = objectId;
        TimestampUtc = timestampUtc;
        TargetUserId = targetUserId;
        OldValues = oldValues;
        NewValues = newValues;
        ClientIp = clientIp;
        ClientInfo = clientInfo;
        ApprovedByUserId = approvedByUserId;
        Reason = reason;
    }

    public Guid Id { get; private set; }
    public string ActorUserId { get; private set; }
    public string ActorDisplayName { get; private set; }
    public string Action { get; private set; }
    public string Module { get; private set; }
    public string ObjectType { get; private set; }
    public string ObjectId { get; private set; }
    public DateTime TimestampUtc { get; private set; }
    public string? TargetUserId { get; private set; }
    public string? OldValues { get; private set; }
    public string? NewValues { get; private set; }
    public string? ClientIp { get; private set; }
    public string? ClientInfo { get; private set; }
    public string? ApprovedByUserId { get; private set; }
    public string? Reason { get; private set; }

    public static AuditLog Create(
        string actorUserId,
        string actorDisplayName,
        string action,
        string module,
        string objectType,
        string objectId,
        DateTime timestampUtc,
        string? targetUserId = null,
        string? oldValues = null,
        string? newValues = null,
        string? clientIp = null,
        string? clientInfo = null,
        string? approvedByUserId = null,
        string? reason = null)
    {
        Guard.AgainstNullOrWhiteSpace(actorUserId, nameof(actorUserId));
        Guard.AgainstNullOrWhiteSpace(actorDisplayName, nameof(actorDisplayName));
        Guard.AgainstNullOrWhiteSpace(action, nameof(action));
        Guard.AgainstNullOrWhiteSpace(module, nameof(module));
        Guard.AgainstNullOrWhiteSpace(objectType, nameof(objectType));
        Guard.AgainstNullOrWhiteSpace(objectId, nameof(objectId));

        return new AuditLog(
            Guid.NewGuid(),
            actorUserId.Trim(),
            actorDisplayName.Trim(),
            action.Trim(),
            module.Trim(),
            objectType.Trim(),
            objectId.Trim(),
            timestampUtc,
            Normalize(targetUserId),
            string.IsNullOrWhiteSpace(oldValues) ? null : oldValues.Trim(),
            string.IsNullOrWhiteSpace(newValues) ? null : newValues.Trim(),
            Normalize(clientIp),
            Normalize(clientInfo),
            Normalize(approvedByUserId),
            Normalize(reason));
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

