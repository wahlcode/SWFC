using SWFC.Application.M200_Business.M204_Inventory.Queries;

namespace SWFC.Application.M200_Business.M204_Inventory.Interfaces;

public interface IStockMovementReadRepository
{
    Task<IReadOnlyList<StockMovementListItem>> GetAllAsync(Guid? stockId, CancellationToken cancellationToken = default);
    Task<StockMovementDetailsDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}