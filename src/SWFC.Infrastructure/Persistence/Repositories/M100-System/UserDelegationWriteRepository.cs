using Microsoft.EntityFrameworkCore;
using SWFC.Application.M100_System.M102_Organization.Users.Delegations;
using SWFC.Domain.M100_System.M102_Organization.Users.Delegations;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M100_System;

public sealed class UserDelegationWriteRepository : IUserDelegationWriteRepository
{
    private readonly AppDbContext _dbContext;

    public UserDelegationWriteRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        UserDelegation delegation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(delegation);

        await _dbContext.Set<UserDelegation>().AddAsync(delegation, cancellationToken);
    }

    public async Task<UserDelegation?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<UserDelegation>()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}
