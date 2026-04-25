using SWFC.Application.M200_Business.M206_Purchasing.PurchaseOrders;
using SWFC.Domain.M200_Business.M206_Purchasing.PurchaseOrders;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M200_Business.M206_Purchasing;

public sealed class PurchaseOrderWriteRepository : IPurchaseOrderWriteRepository
{
    private readonly AppDbContext _dbContext;

    public PurchaseOrderWriteRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(PurchaseOrder purchaseOrder, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<PurchaseOrder>().AddAsync(purchaseOrder, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}