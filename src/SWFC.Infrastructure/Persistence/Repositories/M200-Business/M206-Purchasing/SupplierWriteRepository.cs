using SWFC.Application.M200_Business.M206_Purchasing.Suppliers;
using SWFC.Domain.M200_Business.M206_Purchasing.Suppliers;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M200_Business.M206_Purchasing;

public sealed class SupplierWriteRepository : ISupplierWriteRepository
{
    private readonly AppDbContext _dbContext;

    public SupplierWriteRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Supplier supplier, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<Supplier>().AddAsync(supplier, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
