using Microsoft.EntityFrameworkCore;
using SWFC.Application.M100_System.M103_Authentication.DTOs;
using SWFC.Application.M100_System.M103_Authentication.Interfaces;
using SWFC.Infrastructure.M800_Security.Auth.Entities;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M103_Authentication;

public sealed class LocalCredentialWriteRepository : ILocalCredentialWriteRepository
{
    private readonly AppDbContext _dbContext;

    public LocalCredentialWriteRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LocalCredentialRecordDto?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.LocalCredentials
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

    public async Task AddAsync(
        LocalCredentialRecordDto credential,
        CancellationToken cancellationToken = default)
    {
        var entity = LocalCredential.Create(
            credential.UserId,
            credential.PasswordHash,
            credential.LastPasswordChangedAtUtc);

        if (!credential.IsActive)
        {
            entity.Deactivate();
        }

        for (var i = 0; i < credential.FailedAttempts; i++)
        {
            entity.RecordFailedAttempt(
                DateTimeOffset.UtcNow,
                int.MaxValue,
                TimeSpan.Zero);
        }

        await _dbContext.LocalCredentials.AddAsync(entity, cancellationToken);
    }

    public async Task UpdateAsync(
        LocalCredentialRecordDto credential,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.LocalCredentials
            .FirstOrDefaultAsync(x => x.UserId == credential.UserId, cancellationToken);

        if (entity is null)
        {
            throw new InvalidOperationException("LocalCredential not found.");
        }

        entity.ReplaceState(
            credential.PasswordHash,
            credential.IsActive,
            credential.FailedAttempts,
            credential.LockoutUntilUtc,
            credential.LastPasswordChangedAtUtc);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}