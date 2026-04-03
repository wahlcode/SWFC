using SWFC.Domain.M200_Business.M204_Inventory.Entities;

namespace SWFC.Application.M200_Business.M204_Inventory.Interfaces;

public interface IInventoryItemWriteRepository
{
    Task AddAsync(InventoryItem inventoryItem, CancellationToken cancellationToken = default);
    Task<InventoryItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Remove(InventoryItem inventoryItem);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}