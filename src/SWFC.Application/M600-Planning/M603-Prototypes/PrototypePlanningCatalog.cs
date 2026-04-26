using SWFC.Application.M600_Planning;

namespace SWFC.Application.M600_Planning.M603_Prototypes;

public sealed class PrototypePlanningCatalog : PlanningCatalog
{
    public PrototypePlanningCatalog()
        : base("M603", PlanningRecordKind.Prototype)
    {
    }
}
