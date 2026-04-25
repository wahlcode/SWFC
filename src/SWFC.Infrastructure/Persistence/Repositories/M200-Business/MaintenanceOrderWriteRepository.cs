using SWFC.Application.M200_Business.M202_Maintenance.MaintenanceOrders;
using SWFC.Domain.M200_Business.M202_Maintenance.MaintenanceOrders;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M200_Business;

public sealed class MaintenanceOrderWriteRepository : IMaintenanceOrderWriteRepository
{
    private readonly AppDbContext _dbContext;

    public MaintenanceOrderWriteRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(MaintenanceOrder order, CancellationToken cancellationToken)
    {
        await _dbContext.MaintenanceOrders.AddAsync(order, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
