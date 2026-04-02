using SWFC.Domain.Common.Rules;

namespace SWFC.Domain.M800_Security.M805_AuditCompliance.Entities;

public sealed class AuditLog
{
    private AuditLog()
    {
        Id = Guid.Empty;
        UserId = null!;
        Username = null!;
        Action = null!;
        Entity = null!;
        EntityId = null!;
        TimestampUtc = DateTime.UtcNow;
        OldValues = null;
        NewValues = null;
    }

    private AuditLog(
        Guid id,
        string userId,
        string username,
        string action,
        string entity,
        string entityId,
        DateTime timestampUtc,
        string? oldValues,
        string? newValues)
    {
        Id = id;
        UserId = userId;
        Username = username;
        Action = action;
        Entity = entity;
        EntityId = entityId;
        TimestampUtc = timestampUtc;
        OldValues = oldValues;
        NewValues = newValues;
    }

    public Guid Id { get; private set; }
    public string UserId { get; private set; }
    public string Username { get; private set; }
    public string Action { get; private set; }
    public string Entity { get; private set; }
    public string EntityId { get; private set; }
    public DateTime TimestampUtc { get; private set; }
    public string? OldValues { get; private set; }
    public string? NewValues { get; private set; }

    public static AuditLog Create(
        string userId,
        string username,
        string action,
        string entity,
        string entityId,
        DateTime timestampUtc,
        string? oldValues = null,
        string? newValues = null)
    {
        Guard.AgainstNullOrWhiteSpace(userId, nameof(userId));
        Guard.AgainstNullOrWhiteSpace(username, nameof(username));
        Guard.AgainstNullOrWhiteSpace(action, nameof(action));
        Guard.AgainstNullOrWhiteSpace(entity, nameof(entity));
        Guard.AgainstNullOrWhiteSpace(entityId, nameof(entityId));

        return new AuditLog(
            Guid.NewGuid(),
            userId.Trim(),
            username.Trim(),
            action.Trim(),
            entity.Trim(),
            entityId.Trim(),
            timestampUtc,
            string.IsNullOrWhiteSpace(oldValues) ? null : oldValues.Trim(),
            string.IsNullOrWhiteSpace(newValues) ? null : newValues.Trim());
    }
}