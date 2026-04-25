using Microsoft.EntityFrameworkCore;
using SWFC.Application.M700_Support.M702_ChangeRequests;
using SWFC.Domain.M700_Support.M702_ChangeRequests;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M700_Support;

public sealed class ChangeRequestWriteRepository : IChangeRequestWriteRepository
{
    private readonly AppDbContext _dbContext;

    public ChangeRequestWriteRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(ChangeRequest changeRequest, CancellationToken cancellationToken = default)
    {
        await _dbContext.ChangeRequests.AddAsync(changeRequest, cancellationToken);
    }

    public Task<ChangeRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.ChangeRequests.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
