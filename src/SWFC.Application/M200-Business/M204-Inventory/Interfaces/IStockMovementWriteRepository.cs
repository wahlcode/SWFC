using SWFC.Domain.M200_Business.M204_Inventory.Entities;

namespace SWFC.Application.M200_Business.M204_Inventory.Interfaces;

public interface IStockMovementWriteRepository
{
    Task<Stock?> GetStockByIdAsync(Guid stockId, CancellationToken cancellationToken = default);
    Task AddAsync(StockMovement stockMovement, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}