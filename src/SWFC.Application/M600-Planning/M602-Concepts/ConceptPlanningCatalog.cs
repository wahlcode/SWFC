using SWFC.Application.M600_Planning;

namespace SWFC.Application.M600_Planning.M602_Concepts;

public sealed class ConceptPlanningCatalog : PlanningCatalog
{
    public ConceptPlanningCatalog()
        : base("M602", PlanningRecordKind.Concept)
    {
    }
}
