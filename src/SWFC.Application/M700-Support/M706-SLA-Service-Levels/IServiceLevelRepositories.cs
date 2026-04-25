using SWFC.Domain.M700_Support.M706_SLA_Service_Levels;

namespace SWFC.Application.M700_Support.M706_SLA_Service_Levels;

public interface IServiceLevelReadRepository
{
    Task<IReadOnlyList<ServiceLevel>> GetAllAsync(CancellationToken cancellationToken = default);
}

public interface IServiceLevelWriteRepository
{
    Task AddAsync(ServiceLevel serviceLevel, CancellationToken cancellationToken = default);
    Task<ServiceLevel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
