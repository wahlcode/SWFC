using Microsoft.EntityFrameworkCore;
using SWFC.Application.M200_Business.M206_Purchasing.Suppliers;
using SWFC.Domain.M200_Business.M206_Purchasing.Suppliers;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M200_Business.M206_Purchasing;

public sealed class SupplierReadRepository : ISupplierReadRepository
{
    private readonly AppDbContext _dbContext;

    public SupplierReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<SupplierListItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Supplier>()
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new SupplierListItem(
                x.Id,
                x.Name,
                x.SupplierNumber,
                x.IsActive,
                x.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }
}
