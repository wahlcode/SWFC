using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M200_Business.M201_Assets.Machines;

namespace SWFC.Application.M200_Business.M201_Assets.Machines;

public interface IMachineReadRepository
{
    Task<IReadOnlyList<MachineListItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<MachineDetailsDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MachineSelectionOptionDto>> GetSelectionOptionsAsync(Guid? excludeMachineId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrganizationUnitSelectionOptionDto>> GetOrganizationUnitSelectionOptionsAsync(CancellationToken cancellationToken = default);
}

public interface IMachineWriteRepository
{
    Task AddAsync(Machine machine, CancellationToken cancellationToken = default);
    Task<Machine?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Machine?> GetByIdWithChildrenAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Guid>> GetDescendantIdsAsync(Guid machineId, CancellationToken cancellationToken = default);
    void Deactivate(Machine machine, ChangeContext changeContext);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
