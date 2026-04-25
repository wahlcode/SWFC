using Microsoft.EntityFrameworkCore;
using SWFC.Application.M200_Business.M201_Assets.MachineComponentAreas;
using SWFC.Domain.M200_Business.M201_Assets.MachineComponentAreas;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M200_Business;

public sealed class MachineComponentAreaWriteRepository : IMachineComponentAreaWriteRepository
{
    private readonly AppDbContext _dbContext;

    public MachineComponentAreaWriteRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(MachineComponentArea area, CancellationToken cancellationToken = default)
    {
        return _dbContext.MachineComponentAreas.AddAsync(area, cancellationToken).AsTask();
    }

    public Task<MachineComponentArea?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.MachineComponentAreas
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
