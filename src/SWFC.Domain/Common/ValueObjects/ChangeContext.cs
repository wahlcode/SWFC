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
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId is required.", nameof(userId));

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason is required.", nameof(reason));

        return new ChangeContext(userId.Trim(), reason.Trim());
    }
}