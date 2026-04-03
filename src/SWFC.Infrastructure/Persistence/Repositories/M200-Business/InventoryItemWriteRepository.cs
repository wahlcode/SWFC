using Microsoft.EntityFrameworkCore;
using SWFC.Application.M200_Business.M204_Inventory.Interfaces;
using SWFC.Domain.M200_Business.M204_Inventory.Entities;
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
        return _dbContext.InventoryItems.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public void Remove(InventoryItem inventoryItem)
    {
        _dbContext.InventoryItems.Remove(inventoryItem);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}