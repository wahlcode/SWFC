namespace SWFC.Web.Components.ModuleOverview;

public sealed class ModuleStatusItem
{
    public string ModuleId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Level { get; set; }
    public int ProgressPercent { get; set; }
    public List<ModuleOverviewWorkItemDto> WorkItems { get; set; } = new();
    public string? AuditStatus { get; set; }
    public int? AuditScorePercent { get; set; }
    public string? AuditSource { get; set; }
    public DateTimeOffset? LastAuditUtc { get; set; }
    public string? CodeStatus { get; set; }
    public string? MdStatus { get; set; }
    public string? TestStatus { get; set; }
    public List<string> AuditFlags { get; set; } = new();
    public List<ModuleAuditIssue> AuditIssues { get; set; } = new();
}
