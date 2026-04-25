using Microsoft.EntityFrameworkCore;
using SWFC.Application.M700_Support.M701_BugTracking;
using SWFC.Domain.M700_Support.M701_BugTracking;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M700_Support;

public sealed class BugWriteRepository : IBugWriteRepository
{
    private readonly AppDbContext _dbContext;

    public BugWriteRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Bug bug, CancellationToken cancellationToken = default)
    {
        await _dbContext.Bugs.AddAsync(bug, cancellationToken);
    }

    public Task<Bug?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Bugs.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
