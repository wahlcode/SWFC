using Microsoft.EntityFrameworkCore;
using SWFC.Application.M200_Business.M204_Inventory.Stock;
using SWFC.Domain.M200_Business.M204_Inventory.Reservations;
using StockEntity = SWFC.Domain.M200_Business.M204_Inventory.Stock.Stock;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M200_Business;

public sealed class StockReadRepository : IStockReadRepository
{
    private readonly AppDbContext _dbContext;

    public StockReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<StockEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Stocks
            .AsNoTracking()
            .Include(x => x.Movements)
            .Include(x => x.Reservations)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<StockLookupItem>> GetAllAsync(
        Guid? inventoryItemId = null,
        Guid? locationId = null,
        CancellationToken cancellationToken = default)
    {
        var stocks = await _dbContext.Stocks
            .AsNoTracking()
            .Include(x => x.Reservations)
            .AsQueryable()
            .Where(x => !inventoryItemId.HasValue || x.InventoryItemId == inventoryItemId.Value)
            .Where(x => !locationId.HasValue || x.LocationId == locationId.Value)
            .Join(_dbContext.InventoryItems.AsNoTracking(),
                stock => stock.InventoryItemId,
                item => item.Id,
                (stock, item) => new { stock, item })
            .Join(_dbContext.Locations.AsNoTracking(),
                x => x.stock.LocationId,
                location => location.Id,
                (x, location) => new { x.stock, x.item, location })
            .OrderBy(x => x.item.ArticleNumber.Value)
            .ThenBy(x => x.location.Name.Value)
            .ThenBy(x => x.stock.Bin)
            .ToListAsync(cancellationToken);

        return stocks
            .Select(x =>
            {
                var reservedQuantity = x.stock.Reservations
                    .Where(r => r.Status == StockReservationStatus.Active)
                    .Sum(r => r.Quantity);

                var availableQuantity = x.stock.QuantityOnHand - reservedQuantity;

                return new StockLookupItem(
                    x.stock.Id,
                    x.item.Id,
                    x.item.ArticleNumber.Value,
                    x.item.Name.Value,
                    x.location.Id,
                    x.location.Name.Value,
                    x.location.Code.Value,
                    x.stock.Bin,
                    x.stock.QuantityOnHand,
                    reservedQuantity,
                    availableQuantity);
            })
            .ToList();
    }

    public Task<IReadOnlyList<StockLookupItem>> LookupAsync(CancellationToken cancellationToken = default)
    {
        return GetAllAsync(null, null, cancellationToken);
    }
}
