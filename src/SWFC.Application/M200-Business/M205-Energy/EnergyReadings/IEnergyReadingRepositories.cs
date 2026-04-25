using SWFC.Domain.M200_Business.M205_Energy.EnergyReadings;

namespace SWFC.Application.M200_Business.M205_Energy.EnergyReadings;

public interface IEnergyReadingReadRepository
{
    Task<IReadOnlyList<EnergyReading>> GetByMeterIdAsync(Guid meterId, CancellationToken cancellationToken = default);
    Task<EnergyReading?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IEnergyReadingWriteRepository
{
    Task AddAsync(EnergyReading reading, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
