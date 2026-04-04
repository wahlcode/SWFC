using Microsoft.EntityFrameworkCore;
using SWFC.Application.M200_Business.M201_Assets.Interfaces;
using SWFC.Application.M200_Business.M201_Assets.Queries;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M200_Business;

public sealed class MachineReadRepository : IMachineReadRepository
{
    private readonly AppDbContext _dbContext;

    public MachineReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<MachineListItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var machines = await _dbContext.Machines
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return machines
            .OrderBy(x => x.Name.Value)
            .Select(x => new MachineListItem(
                x.Id,
                x.Name.Value,
                x.InventoryNumber.Value,
                x.Location.Value,
                x.Status.Value,
                x.Manufacturer.Value,
                x.Model.Value,
                x.SerialNumber.Value,
                x.AuditInfo.CreatedAtUtc,
                x.AuditInfo.CreatedBy,
                x.AuditInfo.LastModifiedAtUtc,
                x.AuditInfo.LastModifiedBy))
            .ToList();
    }

    public async Task<MachineDetailsDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var machine = await _dbContext.Machines
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (machine is null)
        {
            return null;
        }

        return new MachineDetailsDto(
            machine.Id,
            machine.Name.Value,
            machine.InventoryNumber.Value,
            machine.Location.Value,
            machine.Status.Value,
            machine.Manufacturer.Value,
            machine.Model.Value,
            machine.SerialNumber.Value,
            machine.Description.Value,
            machine.AuditInfo.CreatedAtUtc,
            machine.AuditInfo.CreatedBy,
            machine.AuditInfo.LastModifiedAtUtc,
            machine.AuditInfo.LastModifiedBy);
    }
}