using Microsoft.EntityFrameworkCore;
using SWFC.Application.M200_Business.M206_Purchasing.PurchaseOrders;
using SWFC.Domain.M200_Business.M206_Purchasing.PurchaseOrders;
using SWFC.Domain.M200_Business.M206_Purchasing.Suppliers;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M200_Business.M206_Purchasing;

public sealed class PurchaseOrderReadRepository : IPurchaseOrderReadRepository
{
    private readonly AppDbContext _dbContext;

    public PurchaseOrderReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<PurchaseOrderListItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<PurchaseOrder>()
            .AsNoTracking()
            .Join(_dbContext.Set<Supplier>().AsNoTracking(),
                order => order.SupplierId,
                supplier => supplier.Id,
                (order, supplier) => new { order, supplier })
            .OrderByDescending(x => x.order.CreatedAtUtc)
            .Select(x => new PurchaseOrderListItem(
                x.order.Id,
                x.order.OrderNumber,
                x.supplier.Id,
                x.supplier.Name,
                x.order.Status,
                x.order.CreatedAtUtc,
                x.order.OrderedAtUtc))
            .ToListAsync(cancellationToken);
    }
}
