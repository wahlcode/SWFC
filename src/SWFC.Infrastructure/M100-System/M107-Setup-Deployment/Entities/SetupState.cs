namespace SWFC.Infrastructure.M100_System.M107_SetupDeployment.Entities;

public sealed class SetupState
{
    public const string SystemStateKey = "system";

    private SetupState()
    {
        Key = string.Empty;
    }

    private SetupState(string key, DateTimeOffset nowUtc)
    {
        Key = key;
        LastCheckedAtUtc = nowUtc;
    }

    public string Key { get; private set; }
    public bool IsConfigured { get; private set; }
    public bool SetupCompleted { get; private set; }
    public bool DatabaseInitialized { get; private set; }
    public bool SetupInProgress { get; private set; }
    public DateTimeOffset LastCheckedAtUtc { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }
    public string? LastFailure { get; private set; }

    public static SetupState Create(DateTimeOffset nowUtc)
    {
        return new SetupState(SystemStateKey, nowUtc);
    }

    public void MarkInProgress(DateTimeOffset nowUtc)
    {
        SetupInProgress = true;
        LastCheckedAtUtc = nowUtc;
        LastFailure = null;
    }

    public void MarkCompleted(DateTimeOffset nowUtc)
    {
        IsConfigured = true;
        SetupCompleted = true;
        DatabaseInitialized = true;
        SetupInProgress = false;
        LastCheckedAtUtc = nowUtc;
        CompletedAtUtc = nowUtc;
        LastFailure = null;
    }

    public void MarkFailed(DateTimeOffset nowUtc, string failure)
    {
        SetupInProgress = false;
        LastCheckedAtUtc = nowUtc;
        LastFailure = string.IsNullOrWhiteSpace(failure)
            ? "Setup failed."
            : failure.Trim();
    }
}
