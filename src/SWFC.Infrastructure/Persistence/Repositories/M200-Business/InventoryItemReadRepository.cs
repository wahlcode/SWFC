using Microsoft.EntityFrameworkCore;
using SWFC.Application.M200_Business.M204_Inventory.Interfaces;
using SWFC.Application.M200_Business.M204_Inventory.Queries;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M200_Business;

public sealed class InventoryItemReadRepository : IInventoryItemReadRepository
{
    private readonly AppDbContext _dbContext;

    public InventoryItemReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<InventoryItemListItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var inventoryItems = await _dbContext.InventoryItems
            .AsNoTracking()
            .Include(x => x.Stock)
            .ToListAsync(cancellationToken);

        return inventoryItems
            .OrderBy(x => x.Name.Value)
            .Select(x => new InventoryItemListItem(
                x.Id,
                x.Name.Value,
                x.Stock?.Id,
                x.Stock?.QuantityOnHand ?? 0,
                x.AuditInfo.CreatedAtUtc,
                x.AuditInfo.CreatedBy,
                x.AuditInfo.LastModifiedAtUtc,
                x.AuditInfo.LastModifiedBy))
            .ToList();
    }

    public async Task<InventoryItemDetailsDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var inventoryItem = await _dbContext.InventoryItems
            .AsNoTracking()
            .Include(x => x.Stock)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (inventoryItem is null)
        {
            return null;
        }

        return new InventoryItemDetailsDto(
            inventoryItem.Id,
            inventoryItem.Name.Value,
            inventoryItem.Stock?.Id,
            inventoryItem.Stock?.QuantityOnHand ?? 0,
            inventoryItem.AuditInfo.CreatedAtUtc,
            inventoryItem.AuditInfo.CreatedBy,
            inventoryItem.AuditInfo.LastModifiedAtUtc,
            inventoryItem.AuditInfo.LastModifiedBy);
    }
}