using SWFC.Application.M200_Business.M205_Energy.EnergyReadings;
using SWFC.Domain.M200_Business.M205_Energy.EnergyReadings;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M200_Business.M205_Energy;

public sealed class EnergyReadingWriteRepository : IEnergyReadingWriteRepository
{
    private readonly AppDbContext _dbContext;

    public EnergyReadingWriteRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(EnergyReading reading, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<EnergyReading>().AddAsync(reading, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
