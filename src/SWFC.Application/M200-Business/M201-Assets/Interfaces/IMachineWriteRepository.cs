using SWFC.Domain.M200_Business.M201_Assets.Entities;

namespace SWFC.Application.M200_Business.M201_Assets.Interfaces;

public interface IMachineWriteRepository
{
    Task AddAsync(Machine machine, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}