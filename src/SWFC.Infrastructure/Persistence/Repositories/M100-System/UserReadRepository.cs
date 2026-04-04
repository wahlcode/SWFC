using Microsoft.EntityFrameworkCore;
using SWFC.Application.M100_System.M102_Organization.Interfaces;
using SWFC.Domain.M100_System.M102_Organization.Entities;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M100_System;

public sealed class UserReadRepository : IUserReadRepository
{
    private readonly AppDbContext _dbContext;

    public UserReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users
            .Include(x => x.Roles)
            .Include(x => x.OrganizationUnits)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<User?> GetByIdentityKeyAsync(string identityKey, CancellationToken cancellationToken = default)
    {
        var user = _dbContext.Users
            .Include(x => x.Roles)
            .Include(x => x.OrganizationUnits)
            .AsEnumerable()
            .FirstOrDefault(x => string.Equals(x.IdentityKey.Value, identityKey, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(user);
    }

    public Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<User> users = _dbContext.Users
            .AsNoTracking()
            .AsEnumerable()
            .OrderBy(x => x.DisplayName.Value, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Task.FromResult(users);
    }
}