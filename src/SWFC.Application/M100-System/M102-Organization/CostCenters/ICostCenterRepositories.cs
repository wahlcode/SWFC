using SWFC.Domain.M100_System.M102_Organization.CostCenters;

namespace SWFC.Application.M100_System.M102_Organization.CostCenters;

public interface ICostCenterReadRepository
{
    Task<CostCenter?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CostCenter?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CostCenter>> GetAllAsync(CancellationToken cancellationToken = default);
}

public interface ICostCenterWriteRepository
{
    Task AddAsync(CostCenter costCenter, CancellationToken cancellationToken = default);
    Task<CostCenter?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CostCenter?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
