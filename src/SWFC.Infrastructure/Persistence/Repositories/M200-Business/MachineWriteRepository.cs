using Microsoft.EntityFrameworkCore;
using SWFC.Application.M200_Business.M201_Assets.Machines;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M200_Business.M201_Assets.Machines;
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

    public Task<Machine?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Machines
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<Machine?> GetByIdWithChildrenAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Machines
            .Include(x => x.Children)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Guid>> GetDescendantIdsAsync(Guid machineId, CancellationToken cancellationToken = default)
    {
        var allMachines = await _dbContext.Machines
            .AsNoTracking()
            .Select(x => new
            {
                x.Id,
                x.ParentMachineId
            })
            .ToListAsync(cancellationToken);

        var descendantIds = new List<Guid>();
        var queue = new Queue<Guid>();

        queue.Enqueue(machineId);

        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();

            var children = allMachines
                .Where(x => x.ParentMachineId == currentId)
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

    public void Deactivate(Machine machine, ChangeContext changeContext)
    {
        machine.Deactivate(changeContext);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
