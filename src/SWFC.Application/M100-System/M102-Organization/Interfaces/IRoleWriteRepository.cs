using SWFC.Domain.M100_System.M102_Organization.Entities;

namespace SWFC.Application.M100_System.M102_Organization.Interfaces;

public interface IRoleWriteRepository
{
    Task AddAsync(Role role, CancellationToken cancellationToken = default);
    Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Role?> GetByNameAsync(string roleName, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}