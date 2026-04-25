using Microsoft.EntityFrameworkCore;
using SWFC.Application.M100_System.M102_Organization.Users;
using SWFC.Domain.M100_System.M102_Organization.Users;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M100_System;

public sealed class UserWriteRepository : IUserWriteRepository
{
    private readonly AppDbContext _dbContext;

    public UserWriteRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _dbContext.Users.AddAsync(user, cancellationToken);
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users
            .Include(x => x.OrganizationUnits)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<User?> GetByIdentityKeyAsync(string identityKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(identityKey))
        {
            return null;
        }

        var normalizedIdentityKey = identityKey.Trim();

        var users = await _dbContext.Users
            .AsNoTracking()
            .Include(x => x.OrganizationUnits)
            .ToListAsync(cancellationToken);

        return users.FirstOrDefault(
            x => x.IdentityKey is not null &&
                 string.Equals(x.IdentityKey.Value, normalizedIdentityKey, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return null;
        }

        var normalizedUsername = username.Trim();

        var users = await _dbContext.Users
            .AsNoTracking()
            .Include(x => x.OrganizationUnits)
            .ToListAsync(cancellationToken);

        return users.FirstOrDefault(
            x => x.Username is not null &&
                 string.Equals(x.Username.Value, normalizedUsername, StringComparison.OrdinalIgnoreCase));
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}