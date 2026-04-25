namespace SWFC.Web.Components.ModuleOverview;

public sealed class ModuleAuditResult
{
    public string ModuleCode { get; set; } = string.Empty;
    public int ScorePercent { get; set; }
    public string Status { get; set; } = ModuleAuditStatuses.NotChecked;
    public string Source { get; set; } = ModuleAuditSources.Generated;
    public DateTimeOffset? LastAuditUtc { get; set; }
    public string CodeStatus { get; set; } = ModuleAuditPartStatuses.NotChecked;
    public string MdStatus { get; set; } = ModuleAuditPartStatuses.NotChecked;
    public string TestStatus { get; set; } = ModuleAuditPartStatuses.NotChecked;
    public List<string> Flags { get; set; } = new();
    public List<ModuleAuditIssue> Issues { get; set; } = new();
    public List<ModuleAuditWorkItemResult> WorkItems { get; set; } = new();
}

public sealed class ModuleAuditIssue
{
    public string Severity { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public sealed class ModuleAuditWorkItemResult
{
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = ModuleWorkItemStatuses.Open;
    public string Evidence { get; set; } = string.Empty;
}

public static class ModuleAuditStatuses
{
    public const string NotChecked = "NotChecked";
    public const string Verified = "Verified";
    public const string PartiallyVerified = "PartiallyVerified";
    public const string Failed = "Failed";
}

public static class ModuleAuditSources
{
    public const string Generated = "Generated";
}

public static class ModuleAuditPartStatuses
{
    public const string NotChecked = "NotChecked";
    public const string Found = "Found";
    public const string Complete = "Complete";
    public const string Incomplete = "Incomplete";
    public const string Missing = "Missing";
}

public static class ModuleAuditFlags
{
    public const string MissingCode = "missing_code";
    public const string MdIncomplete = "md_incomplete";
    public const string MissingTests = "missing_tests";
}
