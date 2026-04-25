using Microsoft.EntityFrameworkCore;
using SWFC.Application.M200_Business.M204_Inventory.Items;
using SWFC.Application.M200_Business.M204_Inventory.Locations;
using SWFC.Application.M200_Business.M204_Inventory.Reservations;
using SWFC.Application.M200_Business.M204_Inventory.Stock;
using SWFC.Application.M200_Business.M204_Inventory.Shared;
using SWFC.Domain.M200_Business.M204_Inventory.Stock;
using SWFC.Domain.M200_Business.M204_Inventory.Reservations;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M200_Business;

public sealed class StockReservationReadRepository : IStockReservationReadRepository
{
    private readonly AppDbContext _dbContext;

    public StockReservationReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<StockReservationListItem>> GetAllAsync(
        Guid? stockId,
        Guid? inventoryItemId,
        Guid? locationId,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.StockReservations
            .AsNoTracking()
            .Join(_dbContext.Stocks.AsNoTracking(),
                reservation => reservation.StockId,
                stock => stock.Id,
                (reservation, stock) => new { reservation, stock })
            .Join(_dbContext.InventoryItems.AsNoTracking(),
                x => x.stock.InventoryItemId,
                item => item.Id,
                (x, item) => new { x.reservation, x.stock, item })
            .Join(_dbContext.Locations.AsNoTracking(),
                x => x.stock.LocationId,
                location => location.Id,
                (x, location) => new { x.reservation, x.stock, x.item, location })
            .AsQueryable();

        if (stockId.HasValue)
        {
            query = query.Where(x => x.stock.Id == stockId.Value);
        }

        if (inventoryItemId.HasValue)
        {
            query = query.Where(x => x.item.Id == inventoryItemId.Value);
        }

        if (locationId.HasValue)
        {
            query = query.Where(x => x.location.Id == locationId.Value);
        }

        return await query
            .OrderByDescending(x => x.reservation.AuditInfo.CreatedAtUtc)
            .Select(x => new StockReservationListItem(
                x.reservation.Id,
                x.stock.Id,
                x.item.Id,
                x.item.Name.Value,
                x.location.Id,
                x.location.Name.Value,
                x.location.Code.Value,
                x.stock.Bin,
                x.reservation.Quantity,
                x.reservation.Note,
                x.reservation.Status.ToString(),
                (int?)x.reservation.TargetType,
                x.reservation.TargetReference,
                x.reservation.AuditInfo.CreatedAtUtc,
                x.reservation.AuditInfo.CreatedBy,
                x.reservation.AuditInfo.LastModifiedAtUtc,
                x.reservation.AuditInfo.LastModifiedBy))
            .ToListAsync(cancellationToken);
    }

    public async Task<StockReservationDetailsDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.StockReservations
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new StockReservationDetailsDto(
                x.Id,
                x.StockId,
                x.Quantity,
                x.Note,
                x.Status.ToString(),
                (int?)x.TargetType,
                x.TargetReference,
                x.AuditInfo.CreatedAtUtc,
                x.AuditInfo.CreatedBy,
                x.AuditInfo.LastModifiedAtUtc,
                x.AuditInfo.LastModifiedBy))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StockLookupItem>> GetStockLookupAsync(
        Guid? inventoryItemId,
        Guid? locationId,
        CancellationToken cancellationToken = default)
    {
        var stocks = await _dbContext.Stocks
            .AsNoTracking()
            .Include(x => x.Reservations)
            .AsQueryable()
            .Where(x => !inventoryItemId.HasValue || x.InventoryItemId == inventoryItemId.Value)
            .Where(x => !locationId.HasValue || x.LocationId == locationId.Value)
            .Join(_dbContext.InventoryItems.AsNoTracking(),
                stock => stock.InventoryItemId,
                item => item.Id,
                (stock, item) => new { stock, item })
            .Join(_dbContext.Locations.AsNoTracking(),
                x => x.stock.LocationId,
                location => location.Id,
                (x, location) => new { x.stock, x.item, location })
            .OrderBy(x => x.item.ArticleNumber.Value)
            .ThenBy(x => x.location.Name.Value)
            .ThenBy(x => x.stock.Bin)
            .ToListAsync(cancellationToken);

        return stocks
            .Select(x =>
            {
                var reservedQuantity = x.stock.Reservations
                    .Where(r => r.Status == StockReservationStatus.Active)
                    .Sum(r => r.Quantity);

                return new StockLookupItem(
                    x.stock.Id,
                    x.item.Id,
                    x.item.ArticleNumber.Value,
                    x.item.Name.Value,
                    x.location.Id,
                    x.location.Name.Value,
                    x.location.Code.Value,
                    x.stock.Bin,
                    x.stock.QuantityOnHand,
                    reservedQuantity,
                    x.stock.QuantityOnHand - reservedQuantity);
            })
            .ToList();
    }
}

