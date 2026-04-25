using Microsoft.EntityFrameworkCore;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.M800_Security.M805_AuditCompliance.Entities;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M800_Security;

public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _dbContext;

    public AuditLogRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        await _dbContext.AuditLogs.AddAsync(auditLog, cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLog>> GetRecentAsync(
        int take,
        CancellationToken cancellationToken = default)
    {
        var normalizedTake = take <= 0 ? 200 : take;

        return await _dbContext.AuditLogs
            .AsNoTracking()
            .OrderByDescending(x => x.TimestampUtc)
            .ThenByDescending(x => x.Id)
            .Take(normalizedTake)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLog>> GetByActorOrTargetUserIdAsync(
        string userId,
        int take,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Array.Empty<AuditLog>();
        }

        var normalizedUserId = userId.Trim();
        var normalizedTake = take <= 0 ? 200 : take;

        return await _dbContext.AuditLogs
            .AsNoTracking()
            .Where(x => x.ActorUserId == normalizedUserId || x.TargetUserId == normalizedUserId)
            .OrderByDescending(x => x.TimestampUtc)
            .ThenByDescending(x => x.Id)
            .Take(normalizedTake)
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}
