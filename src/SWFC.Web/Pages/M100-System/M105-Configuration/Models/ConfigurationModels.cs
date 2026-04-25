namespace SWFC.Web.Pages.M100_System.M105_Configuration.Models;

public sealed record ConfigurationSettingRecord(
    string Key,
    string Value,
    string Scope,
    int Version,
    bool IsSecurityRelevant,
    DateTime UpdatedUtc,
    IReadOnlyList<ConfigurationChangeRecord> History);

public sealed record ModuleActivationRecord(
    string ModuleCode,
    bool IsEnabled,
    int Version,
    DateTime UpdatedUtc,
    string Reason);

public sealed record ConfigurationChangeRecord(
    int Version,
    DateTime TimestampUtc,
    string OldValue,
    string NewValue,
    string Reason);
