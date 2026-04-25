using SWFC.Application.M200_Business.M205_Energy.EnergyMeters;
using SWFC.Domain.M200_Business.M205_Energy.EnergyMeters;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M200_Business.M205_Energy;

public sealed class EnergyMeterWriteRepository : IEnergyMeterWriteRepository
{
    private readonly AppDbContext _dbContext;

    public EnergyMeterWriteRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(EnergyMeter meter, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<EnergyMeter>().AddAsync(meter, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
