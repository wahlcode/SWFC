using SWFC.Application.M200_Business.M206_Purchasing.RequestForQuotations;
using SWFC.Domain.M200_Business.M206_Purchasing.RequestForQuotations;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M200_Business.M206_Purchasing;

public sealed class RequestForQuotationWriteRepository : IRequestForQuotationWriteRepository
{
    private readonly AppDbContext _dbContext;

    public RequestForQuotationWriteRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(RequestForQuotation requestForQuotation, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<RequestForQuotation>().AddAsync(requestForQuotation, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}