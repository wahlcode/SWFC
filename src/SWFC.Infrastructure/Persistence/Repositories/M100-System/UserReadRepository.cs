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
            .AsNoTracking()
            .Include(x => x.Roles)
            .Include(x => x.OrganizationUnits)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<User?> GetByIdentityKeyAsync(string identityKey, CancellationToken cancellationToken = default)
    {
        var normalizedIdentityKey = identityKey.Trim();

        var user = _dbContext.Users
            .AsNoTracking()
            .Include(x => x.Roles)
            .Include(x => x.OrganizationUnits)
            .AsEnumerable()
            .FirstOrDefault(
                x => string.Equals(x.IdentityKey.Value, normalizedIdentityKey, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(user);
    }

    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var normalizedUsername = username.Trim();

        var user = _dbContext.Users
            .AsNoTracking()
            .Include(x => x.Roles)
            .Include(x => x.OrganizationUnits)
            .AsEnumerable()
            .FirstOrDefault(
                x => string.Equals(x.Username.Value, normalizedUsername, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(user);
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Include(x => x.Roles)
            .Include(x => x.OrganizationUnits)
            .ToListAsync(cancellationToken);
    }
}