using Microsoft.EntityFrameworkCore;
using SWFC.Application.M200_Business.M206_Purchasing.GoodsReceipts;
using SWFC.Domain.M200_Business.M204_Inventory.Items;
using SWFC.Domain.M200_Business.M204_Inventory.Locations;
using SWFC.Domain.M200_Business.M206_Purchasing.GoodsReceipts;
using SWFC.Domain.M200_Business.M206_Purchasing.PurchaseOrders;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M200_Business.M206_Purchasing;

public sealed class GoodsReceiptReadRepository : IGoodsReceiptReadRepository
{
    private readonly AppDbContext _dbContext;

    public GoodsReceiptReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<GoodsReceiptListItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<GoodsReceipt>()
            .AsNoTracking()
            .Join(_dbContext.Set<PurchaseOrder>().AsNoTracking(),
                receipt => receipt.PurchaseOrderId,
                order => order.Id,
                (receipt, order) => new { receipt, order })
            .Join(_dbContext.Set<InventoryItem>().AsNoTracking(),
                x => x.receipt.InventoryItemId,
                item => item.Id,
                (x, item) => new { x.receipt, x.order, item })
            .Join(_dbContext.Set<Location>().AsNoTracking(),
                x => x.receipt.LocationId,
                location => location.Id,
                (x, location) => new { x.receipt, x.order, x.item, location })
            .OrderByDescending(x => x.receipt.ReceivedAtUtc)
            .Select(x => new GoodsReceiptListItem(
                x.receipt.Id,
                x.order.Id,
                x.order.OrderNumber,
                x.item.Id,
                x.item.Name.Value,
                x.location.Id,
                x.location.Name.Value,
                x.location.Code.Value,
                x.receipt.Bin,
                x.receipt.Quantity,
                x.receipt.Unit,
                x.receipt.ReceivedAtUtc,
                x.receipt.InventoryBookingStatus,
                x.receipt.InventoryStockMovementId,
                x.receipt.InventoryBookingMessage))
            .ToListAsync(cancellationToken);
    }
}
