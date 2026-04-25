using SWFC.Web.Pages.M100_System.M105_Configuration.Models;

namespace SWFC.Web.Pages.M100_System.M105_Configuration.Services;

public sealed class ConfigurationWorkspaceService
{
    private readonly object _gate = new();
    private readonly Dictionary<string, MutableSetting> _settings = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, ModuleActivationRecord> _moduleActivations = new(StringComparer.OrdinalIgnoreCase);

    public ConfigurationWorkspaceService()
    {
        AddSystemSetting("Localization.DefaultCulture", "en-US", "M306", securityRelevant: false);
        AddSystemSetting("Localization.AllowedCultures", "de-DE,en-US,es-MX,hu-HU,it-IT,pl-PL,ru-RU,sr-RS", "M306", securityRelevant: false);
        AddSystemSetting("Display.TimeZone", "UTC", "M105", securityRelevant: false);
        AddSystemSetting("Audit.RetentionPolicy", "No deletion", "M805", securityRelevant: true);
        AddSystemSetting("Documents.RetentionPolicy", "Versioned history", "M104", securityRelevant: true);
        AddSystemSetting("Search.MaxResults", "50", "M304", securityRelevant: false);

        foreach (var moduleCode in new[] { "M104", "M105", "M302", "M303", "M304" })
        {
            _moduleActivations[moduleCode] = new ModuleActivationRecord(
                moduleCode,
                true,
                1,
                DateTime.UtcNow,
                "Activated for v0.4.0 runtime surface.");
        }
    }

    public IReadOnlyList<ConfigurationSettingRecord> GetSettings()
    {
        lock (_gate)
        {
            return _settings.Values
                .OrderBy(setting => setting.Key, StringComparer.OrdinalIgnoreCase)
                .Select(setting => setting.ToRecord())
                .ToArray();
        }
    }

    public IReadOnlyList<ModuleActivationRecord> GetModuleActivations()
    {
        lock (_gate)
        {
            return _moduleActivations.Values
                .OrderBy(module => module.ModuleCode, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
    }

    public ConfigurationSettingRecord SetSetting(string key, string value, string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        lock (_gate)
        {
            if (!_settings.TryGetValue(key.Trim(), out var setting))
            {
                setting = new MutableSetting
                {
                    Key = key.Trim(),
                    Scope = "M105",
                    IsSecurityRelevant = false,
                    Value = string.Empty,
                    Version = 0,
                    UpdatedUtc = DateTime.UtcNow
                };
                _settings.Add(setting.Key, setting);
            }

            var oldValue = setting.Value;
            setting.Value = value.Trim();
            setting.Version++;
            setting.UpdatedUtc = DateTime.UtcNow;
            setting.History.Add(new ConfigurationChangeRecord(
                setting.Version,
                setting.UpdatedUtc,
                oldValue,
                setting.Value,
                reason.Trim()));

            return setting.ToRecord();
        }
    }

    public ModuleActivationRecord SetModuleActivation(string moduleCode, bool isEnabled, string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(moduleCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        var normalizedModule = moduleCode.Trim().ToUpperInvariant();

        lock (_gate)
        {
            _moduleActivations.TryGetValue(normalizedModule, out var current);
            var next = new ModuleActivationRecord(
                normalizedModule,
                isEnabled,
                (current?.Version ?? 0) + 1,
                DateTime.UtcNow,
                reason.Trim());

            _moduleActivations[normalizedModule] = next;
            return next;
        }
    }

    private void AddSystemSetting(
        string key,
        string value,
        string scope,
        bool securityRelevant)
    {
        var timestamp = DateTime.UtcNow;
        _settings[key] = new MutableSetting
        {
            Key = key,
            Value = value,
            Scope = scope,
            Version = 1,
            IsSecurityRelevant = securityRelevant,
            UpdatedUtc = timestamp,
            History =
            {
                new ConfigurationChangeRecord(
                    1,
                    timestamp,
                    string.Empty,
                    value,
                    "Initial system configuration baseline.")
            }
        };
    }

    private sealed class MutableSetting
    {
        public string Key { get; init; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Scope { get; init; } = string.Empty;
        public int Version { get; set; }
        public bool IsSecurityRelevant { get; init; }
        public DateTime UpdatedUtc { get; set; }
        public List<ConfigurationChangeRecord> History { get; } = new();

        public ConfigurationSettingRecord ToRecord()
        {
            return new ConfigurationSettingRecord(
                Key,
                Value,
                Scope,
                Version,
                IsSecurityRelevant,
                UpdatedUtc,
                History.ToArray());
        }
    }
}
