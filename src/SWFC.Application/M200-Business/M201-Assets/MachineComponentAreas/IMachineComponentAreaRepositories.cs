using SWFC.Domain.M200_Business.M201_Assets.MachineComponentAreas;

namespace SWFC.Application.M200_Business.M201_Assets.MachineComponentAreas;

public interface IMachineComponentAreaReadRepository
{
    Task<IReadOnlyList<MachineComponentAreaListItemDto>> GetAllAsync(CancellationToken cancellationToken = default);
}

public interface IMachineComponentAreaWriteRepository
{
    Task AddAsync(MachineComponentArea area, CancellationToken cancellationToken = default);
    Task<MachineComponentArea?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
