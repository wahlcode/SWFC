using SWFC.Application.M200_Business.M206_Purchasing.GoodsReceipts;
using SWFC.Domain.M200_Business.M206_Purchasing.GoodsReceipts;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M200_Business.M206_Purchasing;

public sealed class GoodsReceiptWriteRepository : IGoodsReceiptWriteRepository
{
    private readonly AppDbContext _dbContext;

    public GoodsReceiptWriteRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(GoodsReceipt goodsReceipt, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<GoodsReceipt>().AddAsync(goodsReceipt, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
