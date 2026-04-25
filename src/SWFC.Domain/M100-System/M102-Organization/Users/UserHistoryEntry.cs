using SWFC.Domain.M100_System.M101_Foundation.Exceptions;

namespace SWFC.Domain.M100_System.M102_Organization.Users;

public sealed class UserHistoryEntry
{
    private UserHistoryEntry()
    {
        Id = Guid.Empty;
        UserId = Guid.Empty;
        ChangeType = UserHistoryChangeType.Created;
        Summary = string.Empty;
        SnapshotJson = string.Empty;
        Reason = string.Empty;
        ChangedAtUtc = DateTime.UtcNow;
        ChangedByUserId = string.Empty;
    }

    private UserHistoryEntry(
        Guid id,
        Guid userId,
        UserHistoryChangeType changeType,
        string summary,
        string snapshotJson,
        string reason,
        DateTime changedAtUtc,
        string changedByUserId)
    {
        Id = id;
        UserId = userId;
        ChangeType = changeType;
        Summary = NormalizeRequired(summary, nameof(summary), 250);
        SnapshotJson = NormalizeRequired(snapshotJson, nameof(snapshotJson), 12000);
        Reason = NormalizeRequired(reason, nameof(reason), 500);
        ChangedAtUtc = changedAtUtc;
        ChangedByUserId = NormalizeRequired(changedByUserId, nameof(changedByUserId), 200);
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public UserHistoryChangeType ChangeType { get; private set; }
    public string Summary { get; private set; }
    public string SnapshotJson { get; private set; }
    public string Reason { get; private set; }
    public DateTime ChangedAtUtc { get; private set; }
    public string ChangedByUserId { get; private set; }

    public static UserHistoryEntry Create(
        Guid userId,
        UserHistoryChangeType changeType,
        string summary,
        string snapshotJson,
        string reason,
        string changedByUserId,
        DateTime changedAtUtc)
    {
        if (userId == Guid.Empty)
        {
            throw new ValidationException("UserId is required.");
        }

        return new UserHistoryEntry(
            Guid.NewGuid(),
            userId,
            changeType,
            summary,
            snapshotJson,
            reason,
            changedAtUtc,
            changedByUserId);
    }

    private static string NormalizeRequired(string? value, string fieldName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException($"{fieldName} is required.");
        }

        var normalized = value.Trim();

        if (normalized.Length > maxLength)
        {
            throw new ValidationException($"{fieldName} must not exceed {maxLength} characters.");
        }

        return normalized;
    }
}
