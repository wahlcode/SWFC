using Microsoft.EntityFrameworkCore;
using SWFC.Application.M200_Business.M202_Maintenance.MaintenancePlans;
using SWFC.Domain.M200_Business.M202_Maintenance.MaintenancePlans;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M200_Business;

public sealed class MaintenancePlanReadRepository : IMaintenancePlanReadRepository
{
    private readonly AppDbContext _dbContext;

    public MaintenancePlanReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<MaintenancePlan>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.MaintenancePlans
            .OrderByDescending(x => x.AuditInfo.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<MaintenancePlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.MaintenancePlans
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}
