using Microsoft.EntityFrameworkCore;
using SWFC.Application.M700_Support.M704_Incident_Management;
using SWFC.Domain.M700_Support.M704_Incident_Management;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M700_Support;

public sealed class IncidentWriteRepository : IIncidentWriteRepository
{
    private readonly AppDbContext _dbContext;

    public IncidentWriteRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Incident incident, CancellationToken cancellationToken = default)
    {
        await _dbContext.Incidents.AddAsync(incident, cancellationToken);
    }

    public Task<Incident?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Incidents.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
