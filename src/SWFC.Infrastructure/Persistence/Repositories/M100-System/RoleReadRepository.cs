using Microsoft.EntityFrameworkCore;
using SWFC.Application.M100_System.M102_Organization.Interfaces;
using SWFC.Domain.M100_System.M102_Organization.Entities;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M100_System;

public sealed class RoleReadRepository : IRoleReadRepository
{
    private readonly AppDbContext _dbContext;

    public RoleReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<Role?> GetByNameAsync(string roleName, CancellationToken cancellationToken = default)
    {
        var role = _dbContext.Roles
            .AsNoTracking()
            .AsEnumerable()
            .FirstOrDefault(x => string.Equals(x.Name.Value, roleName, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(role);
    }

    public Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Role> roles = _dbContext.Roles
            .AsNoTracking()
            .AsEnumerable()
            .OrderBy(x => x.Name.Value, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Task.FromResult(roles);
    }
}