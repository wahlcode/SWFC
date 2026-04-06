using SWFC.Domain.M100_System.M102_Organization.Entities;

namespace SWFC.Application.M100_System.M102_Organization.Interfaces;

public interface IUserWriteRepository
{
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByIdentityKeyAsync(string identityKey, CancellationToken cancellationToken = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}