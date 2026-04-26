using SWFC.Application.M1000_Platform.M1001_PluginSystem;

namespace SWFC.Application.M1000_Platform.M1003_ExtensionManagement;

public enum ExtensionLifecycleStatus
{
    Installed = 0,
    Active = 1,
    Inactive = 2,
    Removed = 3
}

public sealed record ExtensionRegistration(
    string ExtensionId,
    string DisplayName,
    PluginVersion Version,
    string Source,
    DateTimeOffset RegisteredAtUtc);

public sealed record ManagedExtension(
    string ExtensionId,
    string DisplayName,
    PluginVersion Version,
    ExtensionLifecycleStatus Status,
    string Source,
    DateTimeOffset ChangedAtUtc);

public sealed record ExtensionLifecycleAudit(
    string ExtensionId,
    ExtensionLifecycleStatus Status,
    DateTimeOffset OccurredAtUtc,
    string Reason);

public interface IExtensionLifecycleManager
{
    ManagedExtension Install(ExtensionRegistration registration);

    ManagedExtension Activate(string extensionId, DateTimeOffset occurredAtUtc, string reason);

    ManagedExtension Deactivate(string extensionId, DateTimeOffset occurredAtUtc, string reason);

    IReadOnlyList<ManagedExtension> GetExtensions();

    IReadOnlyList<ExtensionLifecycleAudit> GetAuditTrail();
}

public sealed class ExtensionLifecycleManager : IExtensionLifecycleManager
{
    private readonly Dictionary<string, ManagedExtension> _extensions = new(StringComparer.Ordinal);
    private readonly List<ExtensionLifecycleAudit> _auditTrail = [];

    public ManagedExtension Install(ExtensionRegistration registration)
    {
        ArgumentNullException.ThrowIfNull(registration);

        if (string.IsNullOrWhiteSpace(registration.ExtensionId) ||
            string.IsNullOrWhiteSpace(registration.DisplayName) ||
            string.IsNullOrWhiteSpace(registration.Source))
        {
            throw new ArgumentException("Extension id, display name and source are required.");
        }

        if (_extensions.TryGetValue(registration.ExtensionId, out var existing) &&
            existing.Status != ExtensionLifecycleStatus.Removed &&
            existing.Version.CompareTo(registration.Version) == 0)
        {
            throw new InvalidOperationException($"Extension '{registration.ExtensionId}' version {registration.Version} is already installed.");
        }

        var managed = new ManagedExtension(
            registration.ExtensionId.Trim(),
            registration.DisplayName.Trim(),
            registration.Version,
            ExtensionLifecycleStatus.Installed,
            registration.Source.Trim(),
            registration.RegisteredAtUtc);

        _extensions[managed.ExtensionId] = managed;
        _auditTrail.Add(new ExtensionLifecycleAudit(
            managed.ExtensionId,
            managed.Status,
            registration.RegisteredAtUtc,
            "Extension installed and awaiting explicit activation."));
        return managed;
    }

    public ManagedExtension Activate(string extensionId, DateTimeOffset occurredAtUtc, string reason) =>
        ChangeStatus(extensionId, ExtensionLifecycleStatus.Active, occurredAtUtc, reason);

    public ManagedExtension Deactivate(string extensionId, DateTimeOffset occurredAtUtc, string reason) =>
        ChangeStatus(extensionId, ExtensionLifecycleStatus.Inactive, occurredAtUtc, reason);

    public IReadOnlyList<ManagedExtension> GetExtensions() =>
        _extensions.Values
            .OrderBy(extension => extension.ExtensionId, StringComparer.Ordinal)
            .ToArray();

    public IReadOnlyList<ExtensionLifecycleAudit> GetAuditTrail() => _auditTrail.ToArray();

    private ManagedExtension ChangeStatus(
        string extensionId,
        ExtensionLifecycleStatus status,
        DateTimeOffset occurredAtUtc,
        string reason)
    {
        if (!_extensions.TryGetValue(extensionId, out var extension))
        {
            throw new InvalidOperationException($"Extension '{extensionId}' is not installed.");
        }

        if (extension.Status == ExtensionLifecycleStatus.Removed)
        {
            throw new InvalidOperationException($"Extension '{extensionId}' was removed.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Lifecycle changes require a reason.", nameof(reason));
        }

        var changed = extension with { Status = status, ChangedAtUtc = occurredAtUtc };
        _extensions[extensionId] = changed;
        _auditTrail.Add(new ExtensionLifecycleAudit(extensionId, status, occurredAtUtc, reason.Trim()));
        return changed;
    }
}
