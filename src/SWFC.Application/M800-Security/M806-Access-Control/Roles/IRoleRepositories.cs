using SWFC.Domain.M800_Security.M806_AccessControl.Roles;

namespace SWFC.Application.M800_Security.M806_AccessControl.Roles;

public interface IRoleReadRepository
{
    Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Role?> GetByNameAsync(string roleName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken cancellationToken = default);
}

public interface IRoleWriteRepository
{
    Task AddAsync(Role role, CancellationToken cancellationToken = default);
    Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Role?> GetByNameAsync(string roleName, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
