using Microsoft.EntityFrameworkCore;
using SWFC.Application.M800_Security.M803_Visibility;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M800_Security;

public sealed class VisibilityTargetContextRepository : IVisibilityTargetContextRepository
{
    private readonly AppDbContext _dbContext;

    public VisibilityTargetContextRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MachineVisibilityTargetContext?> GetMachineContextAsync(
        Guid machineId,
        CancellationToken cancellationToken = default)
    {
        var exists = await _dbContext.Machines
            .AsNoTracking()
            .AnyAsync(x => x.Id == machineId, cancellationToken);

        return exists
            ? new MachineVisibilityTargetContext(machineId)
            : null;
    }

    public async Task<AreaVisibilityTargetContext?> GetAreaContextAsync(
        Guid areaId,
        CancellationToken cancellationToken = default)
    {
        var exists = await _dbContext.MachineComponentAreas
            .AsNoTracking()
            .AnyAsync(x => x.Id == areaId, cancellationToken);

        return exists
            ? new AreaVisibilityTargetContext(areaId)
            : null;
    }

    public async Task<ComponentVisibilityTargetContext?> GetComponentContextAsync(
        Guid componentId,
        CancellationToken cancellationToken = default)
    {
        var component = await _dbContext.MachineComponents
            .AsNoTracking()
            .Where(x => x.Id == componentId)
            .Select(x => new
            {
                x.Id,
                x.MachineId,
                x.MachineComponentAreaId,
                x.ParentMachineComponentId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (component is null)
        {
            return null;
        }

        var allComponents = await _dbContext.MachineComponents
            .AsNoTracking()
            .Where(x => x.MachineId == component.MachineId)
            .Select(x => new
            {
                x.Id,
                x.ParentMachineComponentId
            })
            .ToListAsync(cancellationToken);

        var parentIds = new List<Guid>();
        var currentParentId = component.ParentMachineComponentId;

        while (currentParentId.HasValue)
        {
            parentIds.Add(currentParentId.Value);

            currentParentId = allComponents
                .Where(x => x.Id == currentParentId.Value)
                .Select(x => x.ParentMachineComponentId)
                .FirstOrDefault();
        }

        return new ComponentVisibilityTargetContext(
            component.Id,
            component.MachineId,
            component.MachineComponentAreaId,
            parentIds);
    }
}
