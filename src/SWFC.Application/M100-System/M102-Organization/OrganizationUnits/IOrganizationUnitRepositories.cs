using SWFC.Domain.M100_System.M102_Organization.OrganizationUnits;

namespace SWFC.Application.M100_System.M102_Organization.OrganizationUnits;

public interface IOrganizationUnitReadRepository
{
    Task<OrganizationUnit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OrganizationUnit?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrganizationUnit>> GetAllAsync(CancellationToken cancellationToken = default);
}

public interface IOrganizationUnitWriteRepository
{
    Task AddAsync(OrganizationUnit unit, CancellationToken cancellationToken = default);
    Task<OrganizationUnit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OrganizationUnit?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
