using Microsoft.EntityFrameworkCore;
using SWFC.Application.M800_Security.M806_AccessControl.Roles;
using SWFC.Domain.M800_Security.M806_AccessControl.Roles;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M800_Security;

public sealed class RoleWriteRepository : IRoleWriteRepository
{
    private readonly AppDbContext _dbContext;

    public RoleWriteRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(Role role, CancellationToken cancellationToken = default)
    {
        return _dbContext.Roles.AddAsync(role, cancellationToken).AsTask();
    }

    public Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Roles
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<Role?> GetByNameAsync(string roleName, CancellationToken cancellationToken = default)
    {
        var role = _dbContext.Roles
            .AsEnumerable()
            .FirstOrDefault(x => string.Equals(x.Name.Value, roleName, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(role);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
