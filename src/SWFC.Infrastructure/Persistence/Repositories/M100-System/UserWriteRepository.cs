using Microsoft.EntityFrameworkCore;
using SWFC.Application.M100_System.M102_Organization.Interfaces;
using SWFC.Domain.M100_System.M102_Organization.Entities;
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

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}