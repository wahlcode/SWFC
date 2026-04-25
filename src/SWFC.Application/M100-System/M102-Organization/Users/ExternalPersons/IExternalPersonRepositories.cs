using SWFC.Domain.M100_System.M102_Organization.Users.ExternalPersons;

namespace SWFC.Application.M100_System.M102_Organization.Users.ExternalPersons;

public interface IExternalPersonReadRepository
{
    Task<ExternalPerson?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExternalPerson>> GetAllAsync(CancellationToken cancellationToken = default);
}

public interface IExternalPersonWriteRepository
{
    Task AddAsync(ExternalPerson externalPerson, CancellationToken cancellationToken = default);
    Task<ExternalPerson?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}