using Microsoft.EntityFrameworkCore;
using SWFC.Application.M200_Business.M206_Purchasing.PurchaseRequirements;
using SWFC.Domain.M200_Business.M206_Purchasing.PurchaseRequirements;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M200_Business.M206_Purchasing;

public sealed class PurchaseRequirementReadRepository : IPurchaseRequirementReadRepository
{
    private readonly AppDbContext _dbContext;

    public PurchaseRequirementReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<PurchaseRequirementDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<PurchaseRequirement>()
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new PurchaseRequirementDto(
                x.Id,
                x.RequiredItem,
                x.Quantity,
                x.Unit,
                x.SourceType,
                x.SourceReferenceId,
                x.Status,
                x.CreatedAtUtc,
                x.DeactivatedAtUtc))
            .ToListAsync(cancellationToken);
    }
}