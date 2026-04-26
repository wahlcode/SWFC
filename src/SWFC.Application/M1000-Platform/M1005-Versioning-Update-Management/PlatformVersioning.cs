using SWFC.Application.M1000_Platform.M1001_PluginSystem;

namespace SWFC.Application.M1000_Platform.M1005_VersioningUpdateManagement;

public enum PlatformUpdateStatus
{
    Planned = 0,
    Compatible = 1,
    Blocked = 2,
    Applied = 3
}

public sealed record PlatformVersionState(
    PluginVersion Version,
    string ReleaseChannel,
    DateTimeOffset ReleasedAtUtc,
    IReadOnlyCollection<string> ChangeReferences);

public sealed record PlatformUpdatePackage(
    string UpdateId,
    PluginVersion FromVersion,
    PluginVersion ToVersion,
    string ReleaseChannel,
    IReadOnlyCollection<string> RequiredApprovals,
    bool RolloutControlled);

public sealed record PlatformUpdateDecision(
    string UpdateId,
    PlatformUpdateStatus Status,
    IReadOnlyCollection<string> Reasons,
    PluginVersion? ResultingVersion);

public interface IPlatformVersioningService
{
    PlatformVersionState RegisterCurrent(PlatformVersionState state);

    PlatformUpdateDecision Evaluate(PlatformUpdatePackage package);

    PlatformUpdateDecision Apply(string updateId, DateTimeOffset appliedAtUtc);

    IReadOnlyList<PlatformVersionState> GetVersionHistory();

    IReadOnlyList<PlatformUpdateDecision> GetUpdateHistory();
}

public sealed class PlatformVersioningService : IPlatformVersioningService
{
    private readonly List<PlatformVersionState> _versionHistory = [];
    private readonly Dictionary<string, PlatformUpdatePackage> _pendingUpdates = new(StringComparer.Ordinal);
    private readonly List<PlatformUpdateDecision> _updateHistory = [];

    public PlatformVersionState RegisterCurrent(PlatformVersionState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        ValidateVersionState(state);
        _versionHistory.Add(Normalize(state));
        return _versionHistory[^1];
    }

    public PlatformUpdateDecision Evaluate(PlatformUpdatePackage package)
    {
        ArgumentNullException.ThrowIfNull(package);

        var reasons = new List<string>();
        if (string.IsNullOrWhiteSpace(package.UpdateId))
        {
            reasons.Add("Update id is required.");
        }

        if (package.ToVersion.CompareTo(package.FromVersion) <= 0)
        {
            reasons.Add("Target version must be greater than source version.");
        }

        if (!package.RolloutControlled)
        {
            reasons.Add("Platform updates require controlled rollout information.");
        }

        if (package.RequiredApprovals.Count == 0)
        {
            reasons.Add("At least one approval reference is required.");
        }

        var latest = _versionHistory.LastOrDefault();
        if (latest is not null && latest.Version.CompareTo(package.FromVersion) != 0)
        {
            reasons.Add($"Current platform version {latest.Version} does not match update source {package.FromVersion}.");
        }

        var status = reasons.Count == 0 ? PlatformUpdateStatus.Compatible : PlatformUpdateStatus.Blocked;
        var decision = new PlatformUpdateDecision(
            package.UpdateId,
            status,
            reasons,
            status == PlatformUpdateStatus.Compatible ? package.ToVersion : null);

        if (status == PlatformUpdateStatus.Compatible)
        {
            _pendingUpdates[package.UpdateId] = package;
        }

        _updateHistory.Add(decision);
        return decision;
    }

    public PlatformUpdateDecision Apply(string updateId, DateTimeOffset appliedAtUtc)
    {
        if (!_pendingUpdates.TryGetValue(updateId, out var package))
        {
            throw new InvalidOperationException($"Update '{updateId}' is not compatible or not evaluated.");
        }

        RegisterCurrent(new PlatformVersionState(
            package.ToVersion,
            package.ReleaseChannel,
            appliedAtUtc,
            package.RequiredApprovals));
        _pendingUpdates.Remove(updateId);

        var decision = new PlatformUpdateDecision(
            updateId,
            PlatformUpdateStatus.Applied,
            ["Update applied after compatibility evaluation."],
            package.ToVersion);
        _updateHistory.Add(decision);
        return decision;
    }

    public IReadOnlyList<PlatformVersionState> GetVersionHistory() => _versionHistory.ToArray();

    public IReadOnlyList<PlatformUpdateDecision> GetUpdateHistory() => _updateHistory.ToArray();

    private static void ValidateVersionState(PlatformVersionState state)
    {
        if (string.IsNullOrWhiteSpace(state.ReleaseChannel) || state.ChangeReferences.Count == 0)
        {
            throw new ArgumentException("Release channel and change references are required.");
        }
    }

    private static PlatformVersionState Normalize(PlatformVersionState state) =>
        state with
        {
            ReleaseChannel = state.ReleaseChannel.Trim(),
            ChangeReferences = state.ChangeReferences
                .Where(reference => !string.IsNullOrWhiteSpace(reference))
                .Select(reference => reference.Trim())
                .Order(StringComparer.Ordinal)
                .ToArray()
        };
}
