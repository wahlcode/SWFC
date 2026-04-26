using SWFC.Application.M600_Planning;

namespace SWFC.Application.M600_Planning.M606_Requirements;

public sealed class RequirementPlanningCatalog : PlanningCatalog
{
    public RequirementPlanningCatalog()
        : base("M606", PlanningRecordKind.Requirement)
    {
    }
}
