using SWFC.Application.M200_Business.M201_Assets.Queries;

namespace SWFC.Application.M200_Business.M201_Assets.Interfaces;

public interface IMachineReadRepository
{
    Task<IReadOnlyList<MachineListItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<MachineDetailsDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}