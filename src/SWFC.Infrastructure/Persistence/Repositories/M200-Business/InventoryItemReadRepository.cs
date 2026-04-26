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

public sealed class InventoryItemReadRepository : IInventoryItemReadRepository
{
    private readonly AppDbContext _dbContext;

    public InventoryItemReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<InventoryItemListItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await _dbContext.InventoryItems
            .AsNoTracking()
            .Include(x => x.Stocks)
                .ThenInclude(x => x.Reservations)
            .ToListAsync(cancellationToken);

        return items
            .OrderBy(x => x.Name.Value)
            .Select(x =>
            {
                var quantityOnHand = x.Stocks.Sum(s => s.QuantityOnHand);
                var reservedQuantity = x.Stocks
                    .SelectMany(s => s.Reservations)
                    .Where(r => r.Status == StockReservationStatus.Active)
                    .Sum(r => r.Quantity);
                var stockValue = quantityOnHand * x.StandardUnitPrice.Value;

                return new InventoryItemListItem(
                    x.Id,
                    x.ArticleNumber.Value,
                    x.Name.Value,
                    x.Description.Value,
                    x.Unit.Value,
                    x.StandardUnitPrice.Value,
                    x.Currency.Value,
                    x.IsActive,
                    x.Stocks.Count,
                    quantityOnHand,
                    reservedQuantity,
                    quantityOnHand - reservedQuantity,
                    stockValue,
                    x.AuditInfo.CreatedAtUtc,
                    x.AuditInfo.CreatedBy,
                    x.AuditInfo.LastModifiedAtUtc,
                    x.AuditInfo.LastModifiedBy);
            })
            .ToList();
    }

    public async Task<InventoryItemDetailsDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await _dbContext.InventoryItems
            .AsNoTracking()
            .Include(x => x.Stocks)
                .ThenInclude(x => x.Reservations)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (item is null)
        {
            return null;
        }

        var locationMap = await _dbContext.Locations
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var stocks = item.Stocks
            .OrderBy(s => locationMap.ContainsKey(s.LocationId) ? locationMap[s.LocationId].Name.Value : string.Empty)
            .ThenBy(s => s.Bin)
            .Select(s =>
            {
                var reserved = s.Reservations
                    .Where(r => r.Status == StockReservationStatus.Active)
                    .Sum(r => r.Quantity);

                locationMap.TryGetValue(s.LocationId, out var location);

                return new InventoryItemStockDto(
                    s.Id,
                    s.LocationId,
                    location?.Name.Value ?? string.Empty,
                    location?.Code.Value ?? string.Empty,
                    s.Bin,
                    s.QuantityOnHand,
                    reserved,
                    s.QuantityOnHand - reserved,
                    s.QuantityOnHand * item.StandardUnitPrice.Value);
            })
            .ToList();

        var totalOnHand = stocks.Sum(x => x.QuantityOnHand);
        var totalReserved = stocks.Sum(x => x.ReservedQuantity);

        return new InventoryItemDetailsDto(
            item.Id,
            item.ArticleNumber.Value,
            item.Name.Value,
            item.Description.Value,
            item.Unit.Value,
            item.Barcode?.Value,
            item.Manufacturer?.Value,
            item.ManufacturerPartNumber?.Value,
            item.StandardUnitPrice.Value,
            item.Currency.Value,
            item.IsActive,
            totalOnHand,
            totalReserved,
            totalOnHand - totalReserved,
            totalOnHand * item.StandardUnitPrice.Value,
            stocks,
            item.AuditInfo.CreatedAtUtc,
            item.AuditInfo.CreatedBy,
            item.AuditInfo.LastModifiedAtUtc,
            item.AuditInfo.LastModifiedBy);
    }

    public async Task<IReadOnlyList<InventoryItemLookupItem>> GetLookupAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.InventoryItems
            .AsNoTracking()
            .OrderBy(x => x.ArticleNumber.Value)
            .ThenBy(x => x.Name.Value)
            .Select(x => new InventoryItemLookupItem(
                x.Id,
                x.ArticleNumber.Value,
                x.Name.Value,
                x.Unit.Value,
                x.IsActive))
            .ToListAsync(cancellationToken);
    }
}

