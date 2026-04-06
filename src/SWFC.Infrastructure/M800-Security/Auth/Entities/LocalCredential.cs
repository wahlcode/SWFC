namespace SWFC.Infrastructure.M800_Security.Auth.Entities;

public sealed class LocalCredential
{
    private LocalCredential()
    {
        Id = Guid.Empty;
        UserId = Guid.Empty;
        PasswordHash = string.Empty;
        IsActive = false;
    }

    private LocalCredential(
        Guid id,
        Guid userId,
        string passwordHash,
        bool isActive,
        int failedAttempts,
        DateTimeOffset? lockoutUntilUtc,
        DateTimeOffset lastPasswordChangedAtUtc)
    {
        Id = id;
        UserId = userId;
        PasswordHash = passwordHash;
        IsActive = isActive;
        FailedAttempts = failedAttempts;
        LockoutUntilUtc = lockoutUntilUtc;
        LastPasswordChangedAtUtc = lastPasswordChangedAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string PasswordHash { get; private set; }
    public bool IsActive { get; private set; }
    public int FailedAttempts { get; private set; }
    public DateTimeOffset? LockoutUntilUtc { get; private set; }
    public DateTimeOffset LastPasswordChangedAtUtc { get; private set; }

    public static LocalCredential Create(
        Guid userId,
        string passwordHash,
        DateTimeOffset nowUtc)
    {
        return new LocalCredential(
            Guid.NewGuid(),
            userId,
            passwordHash,
            isActive: true,
            failedAttempts: 0,
            lockoutUntilUtc: null,
            lastPasswordChangedAtUtc: nowUtc);
    }

    public bool IsLocked(DateTimeOffset nowUtc)
    {
        return LockoutUntilUtc.HasValue && LockoutUntilUtc.Value > nowUtc;
    }

    public void RecordFailedAttempt(DateTimeOffset nowUtc, int lockoutThreshold, TimeSpan lockoutDuration)
    {
        FailedAttempts++;

        if (FailedAttempts >= lockoutThreshold)
        {
            LockoutUntilUtc = nowUtc.Add(lockoutDuration);
            FailedAttempts = 0;
        }
    }

    public void ResetFailedAttempts()
    {
        FailedAttempts = 0;
        LockoutUntilUtc = null;
    }

    public void ChangePassword(string newPasswordHash, DateTimeOffset nowUtc)
    {
        PasswordHash = newPasswordHash;
        LastPasswordChangedAtUtc = nowUtc;
        ResetFailedAttempts();
    }

    public void ReplaceState(
        string passwordHash,
        bool isActive,
        int failedAttempts,
        DateTimeOffset? lockoutUntilUtc,
        DateTimeOffset lastPasswordChangedAtUtc)
    {
        PasswordHash = passwordHash;
        IsActive = isActive;
        FailedAttempts = failedAttempts;
        LockoutUntilUtc = lockoutUntilUtc;
        LastPasswordChangedAtUtc = lastPasswordChangedAtUtc;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}