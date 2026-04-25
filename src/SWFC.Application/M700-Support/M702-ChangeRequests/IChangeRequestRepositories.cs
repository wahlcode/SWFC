using SWFC.Domain.M700_Support.M702_ChangeRequests;

namespace SWFC.Application.M700_Support.M702_ChangeRequests;

public interface IChangeRequestReadRepository
{
    Task<IReadOnlyList<ChangeRequest>> GetAllAsync(CancellationToken cancellationToken = default);
}

public interface IChangeRequestWriteRepository
{
    Task AddAsync(ChangeRequest changeRequest, CancellationToken cancellationToken = default);
    Task<ChangeRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
