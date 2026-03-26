using SWFC.Domain.Common.Rules;

namespace SWFC.Domain.Common.ValueObjects;

public sealed class ChangeContext
{
    private ChangeContext(string userId, string reason)
    {
        UserId = userId;
        Reason = reason;
        ChangedAtUtc = DateTime.UtcNow;
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
