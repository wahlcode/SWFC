namespace SWFC.Application.M800_Security.M803_Visibility;

public interface IVisibilityTargetContextRepository
{
    Task<MachineVisibilityTargetContext?> GetMachineContextAsync(
        Guid machineId,
        CancellationToken cancellationToken = default);

    Task<AreaVisibilityTargetContext?> GetAreaContextAsync(
        Guid areaId,
        CancellationToken cancellationToken = default);

    Task<ComponentVisibilityTargetContext?> GetComponentContextAsync(
        Guid componentId,
        CancellationToken cancellationToken = default);
}
