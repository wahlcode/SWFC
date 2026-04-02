using SWFC.Domain.M200_Business.M201_Assets.Entities;

namespace SWFC.Application.M200_Business.M201_Assets.Interfaces;

public interface IMachineWriteRepository
{
    Task AddAsync(Machine machine, CancellationToken cancellationToken = default);
    Task<Machine?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Remove(Machine machine);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}