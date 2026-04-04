using Microsoft.EntityFrameworkCore;
using SWFC.Application.M200_Business.M204_Inventory.Interfaces;
using SWFC.Domain.M200_Business.M204_Inventory.Entities;
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
            .FirstOrDefaultAsync(x => x.Id == stockId, cancellationToken);
    }

    public async Task AddAsync(StockMovement stockMovement, CancellationToken cancellationToken = default)
    {
        await _dbContext.StockMovements.AddAsync(stockMovement, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddStockAsync(Stock stock, CancellationToken cancellationToken)
    {
        await _dbContext.Stocks.AddAsync(stock, cancellationToken);
    }

    public Task<Stock?> GetStockByInventoryItemIdAsync(Guid inventoryItemId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Stocks
            .Include(x => x.Movements)
            .FirstOrDefaultAsync(x => x.InventoryItemId == inventoryItemId, cancellationToken);
    }
}