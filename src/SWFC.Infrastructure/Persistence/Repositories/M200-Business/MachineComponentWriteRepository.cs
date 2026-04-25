using Microsoft.EntityFrameworkCore;
using SWFC.Application.M200_Business.M201_Assets.MachineComponents;
using SWFC.Domain.M200_Business.M201_Assets.MachineComponents;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M200_Business;

public sealed class MachineComponentWriteRepository : IMachineComponentWriteRepository
{
    private readonly AppDbContext _dbContext;

    public MachineComponentWriteRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(MachineComponent component, CancellationToken cancellationToken = default)
    {
        return _dbContext.MachineComponents.AddAsync(component, cancellationToken).AsTask();
    }

    public Task<MachineComponent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.MachineComponents
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Guid>> GetDescendantIdsAsync(
        Guid componentId,
        CancellationToken cancellationToken = default)
    {
        var allComponents = await _dbContext.MachineComponents
            .AsNoTracking()
            .Select(x => new
            {
                x.Id,
                x.ParentMachineComponentId
            })
            .ToListAsync(cancellationToken);

        var descendantIds = new List<Guid>();
        var queue = new Queue<Guid>();

        queue.Enqueue(componentId);

        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();

            var children = allComponents
                .Where(x => x.ParentMachineComponentId == currentId)
                .Select(x => x.Id)
                .ToList();

            foreach (var childId in children)
            {
                if (descendantIds.Contains(childId))
                {
                    continue;
                }

                descendantIds.Add(childId);
                queue.Enqueue(childId);
            }
        }

        return descendantIds;
    }

    public Task<bool> HasChildrenAsync(Guid componentId, CancellationToken cancellationToken = default)
    {
        return _dbContext.MachineComponents
            .AnyAsync(x => x.ParentMachineComponentId == componentId, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
