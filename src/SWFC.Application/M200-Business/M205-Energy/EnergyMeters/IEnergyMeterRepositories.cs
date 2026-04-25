using SWFC.Domain.M200_Business.M205_Energy.EnergyMeters;

namespace SWFC.Application.M200_Business.M205_Energy.EnergyMeters;

public interface IEnergyMeterReadRepository
{
    Task<IReadOnlyList<EnergyMeter>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<EnergyMeter?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IEnergyMeterWriteRepository
{
    Task AddAsync(EnergyMeter meter, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
