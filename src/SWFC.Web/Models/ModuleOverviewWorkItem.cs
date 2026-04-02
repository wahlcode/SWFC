namespace SWFC.Web.Models;

public sealed class ModuleOverviewWorkItem
{
    public string Title { get; init; } = string.Empty;
    public string Status { get; init; } = "Open";
    public string Level { get; init; } = "Core";
}