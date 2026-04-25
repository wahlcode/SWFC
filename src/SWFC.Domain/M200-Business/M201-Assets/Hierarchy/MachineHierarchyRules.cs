namespace SWFC.Domain.M200_Business.M201_Assets.Hierarchy;

public static class MachineHierarchyRules
{
    public static bool CanAssignParent(
        Guid currentMachineId,
        Guid parentMachineId,
        IReadOnlyCollection<Guid> descendantIds)
    {
        if (currentMachineId == parentMachineId)
        {
            return false;
        }

        if (descendantIds.Contains(parentMachineId))
        {
            return false;
        }

        return true;
    }
}
