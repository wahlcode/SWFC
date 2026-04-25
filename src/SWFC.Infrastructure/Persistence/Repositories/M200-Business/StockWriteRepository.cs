using Microsoft.EntityFrameworkCore;
using SWFC.Application.M200_Business.M204_Inventory.Stock;
using StockEntity = SWFC.Domain.M200_Business.M204_Inventory.Stock.Stock;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M200_Business;

public sealed class StockWriteRepository : IStockWriteRepository
{
    private readonly AppDbContext _dbContext;

    public StockWriteRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<StockEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Stocks
            .Include(x => x.Movements)
            .Include(x => x.Reservations)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task AddAsync(StockEntity stock, CancellationToken cancellationToken = default)
    {
        return _dbContext.Stocks.AddAsync(stock, cancellationToken).AsTask();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
