using SWFC.Domain.M100_System.M102_Organization.Entities;

namespace SWFC.Application.M100_System.M102_Organization.Interfaces;

public interface IOrganizationUnitReadRepository
{
    Task<OrganizationUnit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OrganizationUnit?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrganizationUnit>> GetAllAsync(CancellationToken cancellationToken = default);
}