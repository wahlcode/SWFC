using SWFC.Domain.M100_System.M101_Foundation.Rules;

namespace SWFC.Domain.M100_System.M101_Foundation.ValueObjects;

public sealed class ChangeContext
{
    private ChangeContext(string userId, string reason)
    {
        UserId = userId;
        Reason = reason;
        ChangedAtUtc = UtcTimestamp.Now().UtcDateTime;
    }

    public string UserId { get; }
    public string Reason { get; }
    public DateTime ChangedAtUtc { get; }

    public static ChangeContext Create(string userId, string reason)
    {
        Guard.AgainstNullOrWhiteSpace(userId, nameof(userId));
        Guard.AgainstNullOrWhiteSpace(reason, nameof(reason));

        return new ChangeContext(userId.Trim(), reason.Trim());
    }
}

