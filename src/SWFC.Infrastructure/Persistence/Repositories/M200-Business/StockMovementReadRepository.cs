using Microsoft.EntityFrameworkCore;
using SWFC.Application.M200_Business.M204_Inventory.Interfaces;
using SWFC.Application.M200_Business.M204_Inventory.Queries;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M200_Business;

public sealed class StockMovementReadRepository : IStockMovementReadRepository
{
    private readonly AppDbContext _dbContext;

    public StockMovementReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<StockMovementListItem>> GetAllAsync(
        Guid? stockId,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.StockMovements
            .AsNoTracking()
            .AsQueryable();

        if (stockId.HasValue)
        {
            query = query.Where(x => x.StockId == stockId.Value);
        }

        var movements = await query
            .OrderByDescending(x => x.AuditInfo.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return movements
            .Select(x => new StockMovementListItem(
                x.Id,
                x.StockId,
                x.MovementType,
                x.QuantityDelta,
                x.AuditInfo.CreatedAtUtc,
                x.AuditInfo.CreatedBy))
            .ToList();
    }

    public async Task<StockMovementDetailsDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var movement = await _dbContext.StockMovements
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (movement is null)
        {
            return null;
        }

        return new StockMovementDetailsDto(
            movement.Id,
            movement.StockId,
            movement.MovementType,
            movement.QuantityDelta,
            movement.AuditInfo.CreatedAtUtc,
            movement.AuditInfo.CreatedBy);
    }
}