namespace SWFC.Web.Components.Roadmap;

public sealed class RoadmapMilestone
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = RoadmapStatuses.Planned;
}
