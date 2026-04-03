using SWFC.Application.M200_Business.M204_Inventory.Queries;

namespace SWFC.Application.M200_Business.M204_Inventory.Interfaces;

public interface IInventoryItemReadRepository
{
    Task<IReadOnlyList<InventoryItemListItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<InventoryItemDetailsDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}