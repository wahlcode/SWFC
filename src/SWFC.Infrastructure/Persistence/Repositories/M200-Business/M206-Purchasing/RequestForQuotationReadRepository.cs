using Microsoft.EntityFrameworkCore;
using SWFC.Application.M200_Business.M206_Purchasing.RequestForQuotations;
using SWFC.Domain.M200_Business.M206_Purchasing.PurchaseRequirements;
using SWFC.Domain.M200_Business.M206_Purchasing.RequestForQuotations;
using SWFC.Domain.M200_Business.M206_Purchasing.Suppliers;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M200_Business.M206_Purchasing;

public sealed class RequestForQuotationReadRepository : IRequestForQuotationReadRepository
{
    private readonly AppDbContext _dbContext;

    public RequestForQuotationReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<RequestForQuotationListItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<RequestForQuotation>()
            .AsNoTracking()
            .Join(_dbContext.Set<PurchaseRequirement>().AsNoTracking(),
                rfq => rfq.PurchaseRequirementId,
                requirement => requirement.Id,
                (rfq, requirement) => new { rfq, requirement })
            .Join(_dbContext.Set<Supplier>().AsNoTracking(),
                x => x.rfq.SupplierId,
                supplier => supplier.Id,
                (x, supplier) => new { x.rfq, x.requirement, supplier })
            .OrderByDescending(x => x.rfq.RequestedAtUtc)
            .Select(x => new RequestForQuotationListItem(
                x.rfq.Id,
                x.requirement.Id,
                x.requirement.RequiredItem,
                x.supplier.Id,
                x.supplier.Name,
                x.rfq.RequestedAtUtc,
                x.rfq.ResponseDueAtUtc,
                x.rfq.OfferDocumentReference,
                x.rfq.IsClosed))
            .ToListAsync(cancellationToken);
    }
}
