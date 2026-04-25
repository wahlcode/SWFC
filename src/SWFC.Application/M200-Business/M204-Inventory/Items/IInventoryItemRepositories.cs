using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M200_Business.M204_Inventory.Items;
using SWFC.Domain.M200_Business.M204_Inventory.Locations;
using SWFC.Domain.M200_Business.M204_Inventory.Stock;
using SWFC.Domain.M200_Business.M204_Inventory.Reservations;

namespace SWFC.Application.M200_Business.M204_Inventory.Items;

public interface IInventoryItemReadRepository
{
    Task<IReadOnlyList<InventoryItemListItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<InventoryItemDetailsDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InventoryItemLookupItem>> GetLookupAsync(CancellationToken cancellationToken = default);
}

public interface IInventoryItemWriteRepository
{
    Task AddAsync(InventoryItem inventoryItem, CancellationToken cancellationToken = default);
    Task<InventoryItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Deactivate(InventoryItem inventoryItem, ChangeContext changeContext);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
