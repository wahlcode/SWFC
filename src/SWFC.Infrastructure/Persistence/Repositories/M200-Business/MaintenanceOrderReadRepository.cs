using Microsoft.EntityFrameworkCore;
using SWFC.Application.M200_Business.M202_Maintenance.MaintenanceOrders;
using SWFC.Domain.M200_Business.M202_Maintenance.MaintenanceOrders;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M200_Business;

public sealed class MaintenanceOrderReadRepository : IMaintenanceOrderReadRepository
{
    private readonly AppDbContext _dbContext;

    public MaintenanceOrderReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<MaintenanceOrder>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.MaintenanceOrders
            .Include(x => x.Materials)
            .OrderByDescending(x => x.AuditInfo.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<MaintenanceOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.MaintenanceOrders
            .Include(x => x.Materials)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> NumberExistsAsync(string number, Guid? excludeId, CancellationToken cancellationToken)
    {
        var query = _dbContext.MaintenanceOrders.AsQueryable();

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync(x => x.Number.Value == number.Trim(), cancellationToken);
    }
}
