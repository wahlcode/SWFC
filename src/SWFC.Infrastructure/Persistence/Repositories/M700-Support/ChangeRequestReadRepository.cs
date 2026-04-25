using Microsoft.EntityFrameworkCore;
using SWFC.Application.M700_Support.M702_ChangeRequests;
using SWFC.Domain.M700_Support.M702_ChangeRequests;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M700_Support;

public sealed class ChangeRequestReadRepository : IChangeRequestReadRepository
{
    private readonly AppDbContext _dbContext;

    public ChangeRequestReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ChangeRequest>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.ChangeRequests
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
