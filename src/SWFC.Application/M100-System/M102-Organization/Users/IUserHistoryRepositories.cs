using SWFC.Domain.M100_System.M102_Organization.Users;

namespace SWFC.Application.M100_System.M102_Organization.Users;

public interface IUserHistoryReadRepository
{
    Task<IReadOnlyList<UserHistoryEntry>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}

public interface IUserHistoryWriteRepository
{
    Task AddAsync(UserHistoryEntry entry, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
