namespace SWFC.Web.Models;

public sealed class ModuleOverviewItem
{
    public string Code { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string GroupCode { get; init; } = string.Empty;
    public string GroupTitle { get; init; } = string.Empty;

    public string Status { get; init; } = "Open";
    public string Level { get; init; } = "Core";

    public string Description { get; init; } = string.Empty;

    public List<ModuleOverviewWorkItem> WorkItems { get; init; } = new();

    public int TotalCount => WorkItems.Count == 0 ? 1 : WorkItems.Count;

    public int DoneCount => WorkItems.Count == 0
        ? (Status == "Done" ? 1 : 0)
        : WorkItems.Count(x => x.Status == "Done");

    public int InProgressCount => WorkItems.Count == 0
        ? (Status == "InProgress" ? 1 : 0)
        : WorkItems.Count(x => x.Status == "InProgress");

    public int OpenCount => WorkItems.Count == 0
        ? (Status == "Open" ? 1 : 0)
        : WorkItems.Count(x => x.Status == "Open");

    public int ProgressPercent => TotalCount == 0
        ? 0
        : (int)Math.Round((double)DoneCount / TotalCount * 100, MidpointRounding.AwayFromZero);

    public int CoreCount => WorkItems.Count == 0
        ? (Level == "Core" ? 1 : 0)
        : WorkItems.Count(x => x.Level == "Core");

    public int OptionalCoreCount => WorkItems.Count == 0
        ? (Level == "OptionalCore" ? 1 : 0)
        : WorkItems.Count(x => x.Level == "OptionalCore");

    public int ExtensionCount => WorkItems.Count == 0
        ? (Level == "Extension" ? 1 : 0)
        : WorkItems.Count(x => x.Level == "Extension");
}