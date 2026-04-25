using SWFC.Application.M200_Business.M202_Maintenance.MaintenancePlans;
using SWFC.Domain.M200_Business.M202_Maintenance.MaintenancePlans;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M200_Business;

public sealed class MaintenancePlanWriteRepository : IMaintenancePlanWriteRepository
{
    private readonly AppDbContext _dbContext;

    public MaintenancePlanWriteRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(MaintenancePlan plan, CancellationToken cancellationToken)
    {
        await _dbContext.MaintenancePlans.AddAsync(plan, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
