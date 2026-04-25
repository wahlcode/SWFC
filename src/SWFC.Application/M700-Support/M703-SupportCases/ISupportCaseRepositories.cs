using SWFC.Domain.M700_Support.M703_SupportCases;

namespace SWFC.Application.M700_Support.M703_SupportCases;

public interface ISupportCaseReadRepository
{
    Task<IReadOnlyList<SupportCase>> GetAllAsync(CancellationToken cancellationToken = default);
}

public interface ISupportCaseWriteRepository
{
    Task AddAsync(SupportCase supportCase, CancellationToken cancellationToken = default);
    Task<SupportCase?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
