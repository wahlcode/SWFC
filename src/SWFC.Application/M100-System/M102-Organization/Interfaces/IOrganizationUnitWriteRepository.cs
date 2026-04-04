using SWFC.Domain.M100_System.M102_Organization.Entities;

namespace SWFC.Application.M100_System.M102_Organization.Interfaces;

public interface IOrganizationUnitWriteRepository
{
    Task AddAsync(OrganizationUnit organizationUnit, CancellationToken cancellationToken = default);
    Task<OrganizationUnit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OrganizationUnit?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}