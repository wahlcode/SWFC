using Microsoft.EntityFrameworkCore;
using SWFC.Application.M200_Business.M204_Inventory.Interfaces;
using SWFC.Application.M200_Business.M204_Inventory.Queries;
using SWFC.Domain.M200_Business.M204_Inventory.Enums;
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
                .ThenInclude(x => x.Reservations)
            .ToListAsync(cancellationToken);

        return inventoryItems
            .OrderBy(x => x.Name.Value)
<<<<<<< HEAD
            .Select(x =>
            {
                var reservedQuantity = x.Stock?.Reservations
                    .Where(r => r.Status == StockReservationStatus.Active)
                    .Sum(r => r.Quantity) ?? 0;

                var quantityOnHand = x.Stock?.QuantityOnHand ?? 0;
                var availableQuantity = quantityOnHand - reservedQuantity;

                return new InventoryItemListItem(
                    x.Id,
                    x.Name.Value,
                    x.Stock?.Id,
                    quantityOnHand,
                    reservedQuantity,
                    availableQuantity,
                    x.AuditInfo.CreatedAtUtc,
                    x.AuditInfo.CreatedBy,
                    x.AuditInfo.LastModifiedAtUtc,
                    x.AuditInfo.LastModifiedBy);
            })
=======
            .Select(x => new InventoryItemListItem(
                x.Id,
                x.Name.Value,
                x.Stock?.Id,
                x.Stock?.QuantityOnHand ?? 0,
                x.AuditInfo.CreatedAtUtc,
                x.AuditInfo.CreatedBy,
                x.AuditInfo.LastModifiedAtUtc,
                x.AuditInfo.LastModifiedBy))
>>>>>>> origin/main
            .ToList();
    }

    public async Task<InventoryItemDetailsDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var inventoryItem = await _dbContext.InventoryItems
            .AsNoTracking()
            .Include(x => x.Stock)
                .ThenInclude(x => x.Reservations)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (inventoryItem is null)
        {
            return null;
        }

        var reservedQuantity = inventoryItem.Stock?.Reservations
            .Where(r => r.Status == StockReservationStatus.Active)
            .Sum(r => r.Quantity) ?? 0;

        var quantityOnHand = inventoryItem.Stock?.QuantityOnHand ?? 0;
        var availableQuantity = quantityOnHand - reservedQuantity;

        return new InventoryItemDetailsDto(
            inventoryItem.Id,
            inventoryItem.Name.Value,
            inventoryItem.Stock?.Id,
<<<<<<< HEAD
            quantityOnHand,
            reservedQuantity,
            availableQuantity,
=======
            inventoryItem.Stock?.QuantityOnHand ?? 0,
>>>>>>> origin/main
            inventoryItem.AuditInfo.CreatedAtUtc,
            inventoryItem.AuditInfo.CreatedBy,
            inventoryItem.AuditInfo.LastModifiedAtUtc,
            inventoryItem.AuditInfo.LastModifiedBy);
    }
}