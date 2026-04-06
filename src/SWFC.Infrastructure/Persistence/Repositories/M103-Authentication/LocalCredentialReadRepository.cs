using Microsoft.EntityFrameworkCore;
using SWFC.Application.M100_System.M103_Authentication.DTOs;
using SWFC.Application.M100_System.M103_Authentication.Interfaces;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M103_Authentication;

public sealed class LocalCredentialReadRepository : ILocalCredentialReadRepository
{
    private readonly AppDbContext _dbContext;

    public LocalCredentialReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LocalCredentialRecordDto?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.LocalCredentials
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => new LocalCredentialRecordDto(
                x.UserId,
                x.PasswordHash,
                x.IsActive,
                x.FailedAttempts,
                x.LockoutUntilUtc,
                x.LastPasswordChangedAtUtc))
            .FirstOrDefaultAsync(cancellationToken);
    }
}