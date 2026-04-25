using SWFC.Domain.M700_Support.M701_BugTracking;

namespace SWFC.Application.M700_Support.M701_BugTracking;

public interface IBugReadRepository
{
    Task<IReadOnlyList<Bug>> GetAllAsync(CancellationToken cancellationToken = default);
}

public interface IBugWriteRepository
{
    Task AddAsync(Bug bug, CancellationToken cancellationToken = default);
    Task<Bug?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
