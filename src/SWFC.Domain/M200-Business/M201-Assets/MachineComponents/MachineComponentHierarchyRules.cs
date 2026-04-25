namespace SWFC.Domain.M200_Business.M201_Assets.MachineComponents;

public static class MachineComponentHierarchyRules
{
    public static bool CanAssignParent(
        Guid currentComponentId,
        Guid currentMachineId,
        Guid parentComponentId,
        Guid parentMachineId,
        IReadOnlyCollection<Guid> descendantIds)
    {
        if (currentMachineId != parentMachineId)
        {
            return false;
        }

        if (currentComponentId == parentComponentId)
        {
            return false;
        }

        if (descendantIds.Contains(parentComponentId))
        {
            return false;
        }

        return true;
    }

    public static bool CanAssignParentForNewComponent(
        Guid machineId,
        Guid parentMachineId)
    {
        return machineId == parentMachineId;
    }
}
