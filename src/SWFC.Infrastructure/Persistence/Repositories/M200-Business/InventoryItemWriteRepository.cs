using Microsoft.EntityFrameworkCore;
using SWFC.Application.M200_Business.M204_Inventory.Items;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M200_Business.M204_Inventory.Items;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M200_Business;

public sealed class InventoryItemWriteRepository : IInventoryItemWriteRepository
{
    private readonly AppDbContext _dbContext;

    public InventoryItemWriteRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(InventoryItem inventoryItem, CancellationToken cancellationToken = default)
    {
        await _dbContext.InventoryItems.AddAsync(inventoryItem, cancellationToken);
    }

    public Task<InventoryItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.InventoryItems
            .Include(x => x.Stocks)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public void Deactivate(InventoryItem inventoryItem, ChangeContext changeContext)
    {
        inventoryItem.Deactivate(changeContext);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
