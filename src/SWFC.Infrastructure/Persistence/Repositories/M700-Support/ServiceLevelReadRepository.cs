using Microsoft.EntityFrameworkCore;
using SWFC.Application.M700_Support.M706_SLA_Service_Levels;
using SWFC.Domain.M700_Support.M706_SLA_Service_Levels;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M700_Support;

public sealed class ServiceLevelReadRepository : IServiceLevelReadRepository
{
    private readonly AppDbContext _dbContext;

    public ServiceLevelReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ServiceLevel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.ServiceLevels
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
