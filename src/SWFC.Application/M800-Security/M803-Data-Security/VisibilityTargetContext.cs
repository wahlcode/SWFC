namespace SWFC.Application.M800_Security.M803_Visibility;

public sealed record MachineVisibilityTargetContext(
    Guid MachineId);

public sealed record AreaVisibilityTargetContext(
    Guid AreaId);

public sealed record ComponentVisibilityTargetContext(
    Guid ComponentId,
    Guid MachineId,
    Guid? AreaId,
    IReadOnlyList<Guid> ParentComponentIdsNearestFirst);
