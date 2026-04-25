using Microsoft.EntityFrameworkCore;
using SWFC.Application.M700_Support.M701_BugTracking;
using SWFC.Domain.M700_Support.M701_BugTracking;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M700_Support;

public sealed class BugReadRepository : IBugReadRepository
{
    private readonly AppDbContext _dbContext;

    public BugReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Bug>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Bugs
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
