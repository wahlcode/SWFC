using SWFC.Domain.M200_Business.M201_Assets.MachineComponents;

namespace SWFC.Application.M200_Business.M201_Assets.MachineComponents;

public interface IMachineComponentReadRepository
{
    Task<IReadOnlyList<MachineComponentListItemDto>> GetByMachineAsync(Guid machineId, CancellationToken cancellationToken = default);
}

public interface IMachineComponentWriteRepository
{
    Task AddAsync(MachineComponent component, CancellationToken cancellationToken = default);
    Task<MachineComponent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Guid>> GetDescendantIdsAsync(Guid componentId, CancellationToken cancellationToken = default);
    Task<bool> HasChildrenAsync(Guid componentId, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
