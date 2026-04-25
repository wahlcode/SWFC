using Microsoft.EntityFrameworkCore;
using SWFC.Application.M700_Support.M703_SupportCases;
using SWFC.Domain.M700_Support.M703_SupportCases;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M700_Support;

public sealed class SupportCaseReadRepository : ISupportCaseReadRepository
{
    private readonly AppDbContext _dbContext;

    public SupportCaseReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<SupportCase>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SupportCases
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
