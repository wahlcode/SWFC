using Microsoft.EntityFrameworkCore;
using SWFC.Application.M100_System.M102_Organization.Users;
using SWFC.Domain.M100_System.M102_Organization.Users;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M100_System;

public sealed class UserHistoryReadRepository : IUserHistoryReadRepository
{
    private readonly AppDbContext _dbContext;

    public UserHistoryReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<UserHistoryEntry>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserHistoryEntries
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.ChangedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
