using Microsoft.EntityFrameworkCore;
using SWFC.Application.M200_Business.M201_Assets.MachineComponentAreas;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M200_Business;

public sealed class MachineComponentAreaReadRepository : IMachineComponentAreaReadRepository
{
    private readonly AppDbContext _dbContext;

    public MachineComponentAreaReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<MachineComponentAreaListItemDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var items = await _dbContext.MachineComponentAreas
            .AsNoTracking()
            .Select(x => new MachineComponentAreaListItemDto(
                x.Id,
                x.Name.Value,
                x.IsActive,
                x.AuditInfo.CreatedAtUtc,
                x.AuditInfo.CreatedBy,
                x.AuditInfo.LastModifiedAtUtc,
                x.AuditInfo.LastModifiedBy))
            .ToListAsync(cancellationToken);

        return items
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
