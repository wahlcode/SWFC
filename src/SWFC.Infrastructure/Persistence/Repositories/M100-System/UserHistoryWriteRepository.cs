using SWFC.Application.M100_System.M102_Organization.Users;
using SWFC.Domain.M100_System.M102_Organization.Users;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M100_System;

public sealed class UserHistoryWriteRepository : IUserHistoryWriteRepository
{
    private readonly AppDbContext _dbContext;

    public UserHistoryWriteRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(UserHistoryEntry entry, CancellationToken cancellationToken = default)
    {
        await _dbContext.UserHistoryEntries.AddAsync(entry, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
