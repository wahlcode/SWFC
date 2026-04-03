using Microsoft.EntityFrameworkCore;
using SWFC.Domain.M200_Business.M204_Inventory.Entities;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M200_Business;

public sealed class StockReadRepository
{
    private readonly AppDbContext _dbContext;

    public StockReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Stock?> GetByInventoryItemIdAsync(Guid inventoryItemId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Stocks
            .AsNoTracking()
            .Include(x => x.Movements)
            .FirstOrDefaultAsync(x => x.InventoryItemId == inventoryItemId, cancellationToken);
    }

    public Task<Stock?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Stocks
            .AsNoTracking()
            .Include(x => x.Movements)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}