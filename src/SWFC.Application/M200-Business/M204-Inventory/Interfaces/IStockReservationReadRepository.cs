using SWFC.Application.M200_Business.M204_Inventory.Queries;

namespace SWFC.Application.M200_Business.M204_Inventory.Interfaces;

public interface IStockReservationReadRepository
{
    Task<IReadOnlyList<StockReservationListItem>> GetAllAsync(Guid? stockId, CancellationToken cancellationToken = default);
    Task<StockReservationDetailsDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}