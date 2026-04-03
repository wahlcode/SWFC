using SWFC.Domain.M200_Business.M204_Inventory.Entities;

namespace SWFC.Application.M200_Business.M204_Inventory.Interfaces;

public interface IStockReservationWriteRepository
{
    Task<Stock?> GetStockByIdAsync(Guid stockId, CancellationToken cancellationToken = default);
    Task<StockReservation?> GetByIdAsync(Guid reservationId, CancellationToken cancellationToken = default);
    Task AddAsync(StockReservation reservation, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}