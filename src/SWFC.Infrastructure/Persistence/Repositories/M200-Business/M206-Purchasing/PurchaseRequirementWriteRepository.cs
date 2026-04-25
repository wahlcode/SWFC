using SWFC.Application.M200_Business.M206_Purchasing.PurchaseRequirements;
using SWFC.Domain.M200_Business.M206_Purchasing.PurchaseRequirements;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M200_Business.M206_Purchasing;

public sealed class PurchaseRequirementWriteRepository : IPurchaseRequirementWriteRepository
{
    private readonly AppDbContext _dbContext;

    public PurchaseRequirementWriteRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(PurchaseRequirement purchaseRequirement, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<PurchaseRequirement>().AddAsync(purchaseRequirement, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}