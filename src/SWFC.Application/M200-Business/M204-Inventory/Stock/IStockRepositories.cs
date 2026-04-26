using StockEntity = SWFC.Domain.M200_Business.M204_Inventory.Stock.Stock;
using StockMovementEntity = SWFC.Domain.M200_Business.M204_Inventory.Stock.StockMovement;
using SWFC.Application.M200_Business.M204_Inventory.Reservations;

namespace SWFC.Application.M200_Business.M204_Inventory.Stock;

public interface IStockReadRepository
{
    Task<StockEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StockLookupItem>> GetAllAsync(
        Guid? inventoryItemId = null,
        Guid? locationId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StockLookupItem>> LookupAsync(
        CancellationToken cancellationToken = default);
}

public interface IStockWriteRepository
{
    Task<StockEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        StockEntity stock,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IStockMovementReadRepository
{
    Task<IReadOnlyList<StockMovementListItem>> GetAllAsync(
        Guid? stockId,
        Guid? inventoryItemId,
        Guid? locationId,
        CancellationToken cancellationToken = default);

    Task<StockMovementDetailsDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public interface IStockMovementWriteRepository
{
    Task<StockEntity?> GetStockByIdAsync(
        Guid stockId,
        CancellationToken cancellationToken = default);

    Task<StockEntity?> GetStockByInventoryItemAndLocationAsync(
        Guid inventoryItemId,
        Guid locationId,
        string? bin,
        CancellationToken cancellationToken = default);

    Task<StockEntity?> GetStockByInventoryItemAndLocationForUpdateAsync(
        Guid inventoryItemId,
        Guid locationId,
        string? bin,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        StockMovementEntity stockMovement,
        CancellationToken cancellationToken = default);

    Task AddStockAsync(
        StockEntity stock,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}

