using Microsoft.EntityFrameworkCore;
using SWFC.Application.M100_System.M102_Organization.Users.Delegations;
using SWFC.Domain.M100_System.M102_Organization.Users.Delegations;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M100_System;

public sealed class UserDelegationReadRepository : IUserDelegationReadRepository
{
    private readonly AppDbContext _dbContext;

    public UserDelegationReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserDelegation?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<UserDelegation>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<UserDelegation>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<UserDelegation>()
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.ValidFromUtc)
            .ThenBy(x => x.DelegateUserId)
            .ToListAsync(cancellationToken);
    }
}
