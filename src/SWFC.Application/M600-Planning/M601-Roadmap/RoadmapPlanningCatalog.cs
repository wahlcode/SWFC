using SWFC.Application.M600_Planning;

namespace SWFC.Application.M600_Planning.M601_Roadmap;

public sealed class RoadmapPlanningCatalog : PlanningCatalog
{
    public RoadmapPlanningCatalog()
        : base("M601", PlanningRecordKind.Roadmap)
    {
    }
}
