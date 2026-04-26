using SWFC.Application.M600_Planning;

namespace SWFC.Application.M600_Planning.M605_Standards_Guidelines;

public sealed class StandardPlanningCatalog : PlanningCatalog
{
    public StandardPlanningCatalog()
        : base("M605", PlanningRecordKind.Standard)
    {
    }
}
