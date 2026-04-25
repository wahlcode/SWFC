namespace SWFC.Web.Components.Roadmap;

public sealed class RoadmapFileDto
{
    public List<RoadmapVersion> Versions { get; set; } = new();
}

public sealed class RoadmapVersion
{
    public string Version { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string AudienceTitle { get; set; } = string.Empty;
    public string Goal { get; set; } = string.Empty;
    public string UserDescription { get; set; } = string.Empty;
    public string DeveloperDescription { get; set; } = string.Empty;
    public string Status { get; set; } = RoadmapStatuses.Planned;
    public List<string> PrimaryModules { get; set; } = new();
    public List<string> RequiredModules { get; set; } = new();
    public List<RoadmapMilestone> Milestones { get; set; } = new();
    public string Result { get; set; } = string.Empty;
}

public static class RoadmapStatuses
{
    public const string Done = "Done";
    public const string InProgress = "InProgress";
    public const string Planned = "Planned";
}
