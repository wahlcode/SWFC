using Microsoft.EntityFrameworkCore;
using SWFC.Application.M200_Business.M205_Energy.EnergyMeters;
using SWFC.Domain.M200_Business.M205_Energy.EnergyMeters;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M200_Business.M205_Energy;

public sealed class EnergyMeterReadRepository : IEnergyMeterReadRepository
{
    private readonly AppDbContext _dbContext;

    public EnergyMeterReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<EnergyMeter>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<EnergyMeter>()
            .AsNoTracking()
            .OrderBy(x => x.Name.Value)
            .ToListAsync(cancellationToken);
    }

    public Task<EnergyMeter?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Set<EnergyMeter>()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}
