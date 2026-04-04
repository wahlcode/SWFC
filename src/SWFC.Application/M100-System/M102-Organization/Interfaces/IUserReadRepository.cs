using SWFC.Domain.M100_System.M102_Organization.Entities;

namespace SWFC.Application.M100_System.M102_Organization.Interfaces;

public interface IUserReadRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByIdentityKeyAsync(string identityKey, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);
}