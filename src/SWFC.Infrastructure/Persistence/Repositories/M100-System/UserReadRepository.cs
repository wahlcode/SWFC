using Microsoft.EntityFrameworkCore;
using SWFC.Application.M100_System.M102_Organization.Users;
using SWFC.Domain.M100_System.M102_Organization.Users;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M100_System;

public sealed class UserReadRepository : IUserReadRepository
{
    private readonly AppDbContext _dbContext;

    public UserReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Include(x => x.OrganizationUnits)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<User?> GetByIdentityKeyAsync(
        string identityKey,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(identityKey))
        {
            return null;
        }

        var normalizedIdentityKey = identityKey.Trim().ToUpperInvariant();

        var users = await _dbContext.Users
            .AsNoTracking()
            .Include(x => x.OrganizationUnits)
            .ToListAsync(cancellationToken);

        return users.FirstOrDefault(
            x => x.IdentityKey is not null &&
                 !string.IsNullOrWhiteSpace(x.IdentityKey.Value) &&
                 string.Equals(
                     x.IdentityKey.Value.Trim(),
                     normalizedIdentityKey,
                     StringComparison.OrdinalIgnoreCase));
    }

    public async Task<User?> GetByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return null;
        }

        var normalizedUsername = username.Trim().ToUpperInvariant();

        var users = await _dbContext.Users
            .AsNoTracking()
            .Include(x => x.OrganizationUnits)
            .ToListAsync(cancellationToken);

        return users.FirstOrDefault(
            x => x.Username is not null &&
                 !string.IsNullOrWhiteSpace(x.Username.Value) &&
                 string.Equals(
                     x.Username.Value.Trim(),
                     normalizedUsername,
                     StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Include(x => x.OrganizationUnits)
            .ToListAsync(cancellationToken);
    }
}
