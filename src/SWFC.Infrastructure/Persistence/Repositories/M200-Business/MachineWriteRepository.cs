using SWFC.Application.M200_Business.M201_Assets.Interfaces;
using SWFC.Domain.M200_Business.M201_Assets.Entities;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M200_Business;

public sealed class MachineWriteRepository : IMachineWriteRepository
{
    private readonly AppDbContext _dbContext;

    public MachineWriteRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Machine machine, CancellationToken cancellationToken = default)
    {
        await _dbContext.Machines.AddAsync(machine, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}