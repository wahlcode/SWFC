namespace SWFC.Application.M1000_Platform.M1001_PluginSystem;

public enum PluginLifecycleState
{
    Registered = 0,
    Loaded = 1,
    Failed = 2,
    Unloaded = 3
}

public sealed record PluginVersion(int Major, int Minor, int Patch) : IComparable<PluginVersion>
{
    public int CompareTo(PluginVersion? other)
    {
        if (other is null)
        {
            return 1;
        }

        var major = Major.CompareTo(other.Major);
        if (major != 0)
        {
            return major;
        }

        var minor = Minor.CompareTo(other.Minor);
        return minor != 0 ? minor : Patch.CompareTo(other.Patch);
    }

    public override string ToString() => $"{Major}.{Minor}.{Patch}";
}

public sealed record PluginCapability(string Code, string ExtensionPoint, bool RequiresRuntimeAccess);

public sealed record PluginManifest(
    string PluginId,
    string DisplayName,
    PluginVersion Version,
    IReadOnlyCollection<PluginCapability> Capabilities,
    PluginVersion MinimumPlatformVersion,
    PluginVersion MaximumPlatformVersion);

public sealed record PluginRuntimeContext(
    string PluginId,
    string ExtensionPoint,
    IReadOnlyDictionary<string, string> Settings);

public sealed record PluginLoadResult(
    string PluginId,
    PluginLifecycleState State,
    bool IsolatedFromCore,
    IReadOnlyCollection<string> BoundExtensionPoints,
    string Evidence);

public sealed record PluginLifecycleEvent(
    string PluginId,
    PluginLifecycleState State,
    DateTimeOffset OccurredAtUtc,
    string Evidence);

public interface IPlatformPlugin
{
    PluginManifest Manifest { get; }

    void Initialize(PluginRuntimeContext context);

    void Shutdown();
}

public interface IPluginCatalog
{
    void RegisterExtensionPoint(string extensionPoint);

    PluginLoadResult Load(IPlatformPlugin plugin, PluginVersion platformVersion, DateTimeOffset occurredAtUtc);

    PluginLifecycleEvent Unload(string pluginId, DateTimeOffset occurredAtUtc);

    IReadOnlyList<PluginLoadResult> GetLoadedPlugins();

    IReadOnlyList<PluginLifecycleEvent> GetLifecycleHistory();
}

public sealed class PluginCatalog : IPluginCatalog
{
    private readonly SortedSet<string> _extensionPoints = new(StringComparer.Ordinal);
    private readonly Dictionary<string, LoadedPluginEntry> _loadedPlugins = new(StringComparer.Ordinal);
    private readonly List<PluginLifecycleEvent> _history = [];

    public void RegisterExtensionPoint(string extensionPoint)
    {
        if (string.IsNullOrWhiteSpace(extensionPoint))
        {
            throw new ArgumentException("Extension point is required.", nameof(extensionPoint));
        }

        _extensionPoints.Add(extensionPoint.Trim());
    }

    public PluginLoadResult Load(
        IPlatformPlugin plugin,
        PluginVersion platformVersion,
        DateTimeOffset occurredAtUtc)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        ArgumentNullException.ThrowIfNull(platformVersion);

        var manifest = plugin.Manifest;
        ValidateManifest(manifest);

        var capabilityPoints = manifest.Capabilities
            .Select(capability => capability.ExtensionPoint.Trim())
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        var missingExtensionPoint = capabilityPoints.FirstOrDefault(point => !_extensionPoints.Contains(point));
        if (missingExtensionPoint is not null)
        {
            return Fail(manifest.PluginId, occurredAtUtc, $"Unknown extension point '{missingExtensionPoint}'.");
        }

        if (platformVersion.CompareTo(manifest.MinimumPlatformVersion) < 0 ||
            platformVersion.CompareTo(manifest.MaximumPlatformVersion) > 0)
        {
            return Fail(
                manifest.PluginId,
                occurredAtUtc,
                $"Platform version {platformVersion} is outside supported range {manifest.MinimumPlatformVersion}-{manifest.MaximumPlatformVersion}.");
        }

        if (manifest.Capabilities.Any(capability => capability.RequiresRuntimeAccess))
        {
            return Fail(manifest.PluginId, occurredAtUtc, "Plugin requested direct runtime access outside platform contracts.");
        }

        try
        {
            var context = new PluginRuntimeContext(
                manifest.PluginId,
                string.Join(";", capabilityPoints),
                new SortedDictionary<string, string>(StringComparer.Ordinal)
                {
                    ["Isolation"] = "CoreContractsOnly",
                    ["PlatformApi"] = "M1001"
                });
            plugin.Initialize(context);
        }
        catch (Exception ex)
        {
            return Fail(manifest.PluginId, occurredAtUtc, $"Plugin initialization failed: {ex.GetType().Name}.");
        }

        var result = new PluginLoadResult(
            manifest.PluginId,
            PluginLifecycleState.Loaded,
            IsolatedFromCore: true,
            capabilityPoints,
            "Plugin loaded through registered extension points and contract-only runtime context.");

        _loadedPlugins[manifest.PluginId] = new LoadedPluginEntry(plugin, result);
        _history.Add(new PluginLifecycleEvent(
            manifest.PluginId,
            PluginLifecycleState.Loaded,
            occurredAtUtc,
            result.Evidence));

        return result;
    }

    public PluginLifecycleEvent Unload(string pluginId, DateTimeOffset occurredAtUtc)
    {
        if (!_loadedPlugins.TryGetValue(pluginId, out var entry))
        {
            throw new InvalidOperationException($"Plugin '{pluginId}' is not loaded.");
        }

        entry.Plugin.Shutdown();
        _loadedPlugins.Remove(pluginId);

        var lifecycleEvent = new PluginLifecycleEvent(
            pluginId,
            PluginLifecycleState.Unloaded,
            occurredAtUtc,
            "Plugin unloaded through platform lifecycle control.");
        _history.Add(lifecycleEvent);
        return lifecycleEvent;
    }

    public IReadOnlyList<PluginLoadResult> GetLoadedPlugins() =>
        _loadedPlugins.Values
            .Select(entry => entry.Result)
            .OrderBy(entry => entry.PluginId, StringComparer.Ordinal)
            .ToArray();

    public IReadOnlyList<PluginLifecycleEvent> GetLifecycleHistory() => _history.ToArray();

    private PluginLoadResult Fail(string pluginId, DateTimeOffset occurredAtUtc, string evidence)
    {
        var result = new PluginLoadResult(
            pluginId,
            PluginLifecycleState.Failed,
            IsolatedFromCore: true,
            Array.Empty<string>(),
            evidence);
        _history.Add(new PluginLifecycleEvent(pluginId, PluginLifecycleState.Failed, occurredAtUtc, evidence));
        return result;
    }

    private static void ValidateManifest(PluginManifest manifest)
    {
        if (string.IsNullOrWhiteSpace(manifest.PluginId) ||
            string.IsNullOrWhiteSpace(manifest.DisplayName) ||
            manifest.Capabilities.Count == 0)
        {
            throw new ArgumentException("Plugin id, display name and capabilities are required.");
        }

        if (manifest.Capabilities.Any(capability =>
                string.IsNullOrWhiteSpace(capability.Code) ||
                string.IsNullOrWhiteSpace(capability.ExtensionPoint)))
        {
            throw new ArgumentException("Plugin capabilities require code and extension point.");
        }
    }

    private sealed record LoadedPluginEntry(IPlatformPlugin Plugin, PluginLoadResult Result);
}
