using Microsoft.EntityFrameworkCore;
using SWFC.Application.M700_Support.M704_Incident_Management;
using SWFC.Domain.M700_Support.M704_Incident_Management;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M700_Support;

public sealed class IncidentReadRepository : IIncidentReadRepository
{
    private readonly AppDbContext _dbContext;

    public IncidentReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Incident>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Incidents
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
