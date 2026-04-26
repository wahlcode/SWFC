using SWFC.Application.M600_Planning;

namespace SWFC.Application.M600_Planning.M604_Decisions;

public sealed class DecisionPlanningCatalog : PlanningCatalog
{
    public DecisionPlanningCatalog()
        : base("M604", PlanningRecordKind.Decision)
    {
    }
}
