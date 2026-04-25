using Microsoft.EntityFrameworkCore;
using SWFC.Application.M200_Business.M204_Inventory.Items;
using SWFC.Application.M200_Business.M204_Inventory.Locations;
using SWFC.Application.M200_Business.M204_Inventory.Reservations;
using SWFC.Application.M200_Business.M204_Inventory.Stock;
using SWFC.Application.M200_Business.M204_Inventory.Shared;
using SWFC.Domain.M200_Business.M204_Inventory.Items;
using SWFC.Domain.M200_Business.M204_Inventory.Locations;
using SWFC.Domain.M200_Business.M204_Inventory.Stock;
using SWFC.Domain.M200_Business.M204_Inventory.Reservations;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M200_Business;

public sealed class StockMovementWriteRepository : IStockMovementWriteRepository
{
    private readonly AppDbContext _dbContext;

    public StockMovementWriteRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Stock?> GetStockByIdAsync(Guid stockId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Stocks
            .Include(x => x.Movements)
            .Include(x => x.Reservations)
            .FirstOrDefaultAsync(x => x.Id == stockId, cancellationToken);
    }

    public Task<Stock?> GetStockByInventoryItemIdAsync(Guid inventoryItemId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Stocks
            .Include(x => x.Movements)
            .Include(x => x.Reservations)
            .FirstOrDefaultAsync(x => x.InventoryItemId == inventoryItemId, cancellationToken);
    }

    public Task<Stock?> GetStockByInventoryItemAndLocationAsync(
        Guid inventoryItemId,
        Guid locationId,
        string? bin,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Stocks
            .Include(x => x.Movements)
            .Include(x => x.Reservations)
            .FirstOrDefaultAsync(
                x => x.InventoryItemId == inventoryItemId
                    && x.LocationId == locationId
                    && x.Bin == bin,
                cancellationToken);
    }

    public Task AddAsync(StockMovement stockMovement, CancellationToken cancellationToken = default)
    {
        return _dbContext.StockMovements.AddAsync(stockMovement, cancellationToken).AsTask();
    }

    public Task AddStockAsync(Stock stock, CancellationToken cancellationToken)
    {
        return _dbContext.Stocks.AddAsync(stock, cancellationToken).AsTask();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}

