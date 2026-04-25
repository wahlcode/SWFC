using Microsoft.EntityFrameworkCore;
using SWFC.Application.M700_Support.M706_SLA_Service_Levels;
using SWFC.Domain.M700_Support.M706_SLA_Service_Levels;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M700_Support;

public sealed class ServiceLevelWriteRepository : IServiceLevelWriteRepository
{
    private readonly AppDbContext _dbContext;

    public ServiceLevelWriteRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(ServiceLevel serviceLevel, CancellationToken cancellationToken = default)
    {
        await _dbContext.ServiceLevels.AddAsync(serviceLevel, cancellationToken);
    }

    public Task<ServiceLevel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.ServiceLevels.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
