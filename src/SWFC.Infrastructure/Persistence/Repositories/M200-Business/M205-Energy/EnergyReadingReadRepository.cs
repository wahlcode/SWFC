using Microsoft.EntityFrameworkCore;
using SWFC.Application.M200_Business.M205_Energy.EnergyReadings;
using SWFC.Domain.M200_Business.M205_Energy.EnergyReadings;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M200_Business.M205_Energy;

public sealed class EnergyReadingReadRepository : IEnergyReadingReadRepository
{
    private readonly AppDbContext _dbContext;

    public EnergyReadingReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<EnergyReading>> GetByMeterIdAsync(
        Guid meterId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<EnergyReading>()
            .AsNoTracking()
            .Where(x => x.MeterId == meterId)
            .OrderByDescending(x => x.Date.Value)
            .ThenByDescending(x => x.AuditInfo.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task<EnergyReading?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Set<EnergyReading>()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}
