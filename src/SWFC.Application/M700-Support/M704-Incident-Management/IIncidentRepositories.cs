using SWFC.Domain.M700_Support.M704_Incident_Management;

namespace SWFC.Application.M700_Support.M704_Incident_Management;

public interface IIncidentReadRepository
{
    Task<IReadOnlyList<Incident>> GetAllAsync(CancellationToken cancellationToken = default);
}

public interface IIncidentWriteRepository
{
    Task AddAsync(Incident incident, CancellationToken cancellationToken = default);
    Task<Incident?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
