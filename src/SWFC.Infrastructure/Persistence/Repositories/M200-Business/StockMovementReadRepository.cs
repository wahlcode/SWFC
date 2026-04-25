using Microsoft.EntityFrameworkCore;
using SWFC.Application.M200_Business.M204_Inventory.Stock;
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
        Guid? inventoryItemId,
        Guid? locationId,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.StockMovements
            .AsNoTracking()
            .Join(_dbContext.Stocks.AsNoTracking(),
                movement => movement.StockId,
                stock => stock.Id,
                (movement, stock) => new { movement, stock })
            .Join(_dbContext.InventoryItems.AsNoTracking(),
                x => x.stock.InventoryItemId,
                item => item.Id,
                (x, item) => new { x.movement, x.stock, item })
            .Join(_dbContext.Locations.AsNoTracking(),
                x => x.stock.LocationId,
                location => location.Id,
                (x, location) => new { x.movement, x.stock, x.item, location })
            .AsQueryable();

        if (stockId.HasValue)
        {
            query = query.Where(x => x.stock.Id == stockId.Value);
        }

        if (inventoryItemId.HasValue)
        {
            query = query.Where(x => x.item.Id == inventoryItemId.Value);
        }

        if (locationId.HasValue)
        {
            query = query.Where(x => x.location.Id == locationId.Value);
        }

        return await query
            .OrderByDescending(x => x.movement.AuditInfo.CreatedAtUtc)
            .Select(x => new StockMovementListItem(
                x.movement.Id,
                x.stock.Id,
                x.item.Id,
                x.item.Name.Value,
                x.location.Id,
                x.location.Name.Value,
                x.location.Code.Value,
                x.stock.Bin,
                x.movement.MovementType,
                x.movement.QuantityDelta,
                x.movement.TargetType,
                x.movement.TargetReference,
                x.movement.AuditInfo.CreatedAtUtc,
                x.movement.AuditInfo.CreatedBy))
            .ToListAsync(cancellationToken);
    }

    public async Task<StockMovementDetailsDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.StockMovements
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new StockMovementDetailsDto(
                x.Id,
                x.StockId,
                x.MovementType,
                x.QuantityDelta,
                x.TargetType,
                x.TargetReference,
                x.AuditInfo.CreatedAtUtc,
                x.AuditInfo.CreatedBy))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
