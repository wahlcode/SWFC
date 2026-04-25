using StockEntity = SWFC.Domain.M200_Business.M204_Inventory.Stock.Stock;
using StockReservationEntity = SWFC.Domain.M200_Business.M204_Inventory.Reservations.StockReservation;
using SWFC.Application.M200_Business.M204_Inventory.Stock;

namespace SWFC.Application.M200_Business.M204_Inventory.Reservations;

public interface IStockReservationReadRepository
{
    Task<IReadOnlyList<StockReservationListItem>> GetAllAsync(
        Guid? stockId,
        Guid? inventoryItemId,
        Guid? locationId,
        CancellationToken cancellationToken = default);

    Task<StockReservationDetailsDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StockLookupItem>> GetStockLookupAsync(
        Guid? inventoryItemId,
        Guid? locationId,
        CancellationToken cancellationToken = default);
}

public interface IStockReservationWriteRepository
{
    Task<StockEntity?> GetStockByIdAsync(
        Guid stockId,
        CancellationToken cancellationToken = default);

    Task<StockReservationEntity?> GetByIdAsync(
        Guid reservationId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        StockReservationEntity reservation,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

