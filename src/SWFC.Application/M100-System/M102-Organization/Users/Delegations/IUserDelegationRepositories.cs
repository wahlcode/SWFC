using SWFC.Domain.M100_System.M102_Organization.Users.Delegations;

namespace SWFC.Application.M100_System.M102_Organization.Users.Delegations;

public interface IUserDelegationReadRepository
{
    Task<UserDelegation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserDelegation>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}

public interface IUserDelegationWriteRepository
{
    Task AddAsync(UserDelegation delegation, CancellationToken cancellationToken = default);
    Task<UserDelegation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}