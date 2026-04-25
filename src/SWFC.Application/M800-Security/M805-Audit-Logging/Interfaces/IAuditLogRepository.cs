using SWFC.Domain.M800_Security.M805_AuditCompliance.Entities;

namespace SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditLog>> GetRecentAsync(
        int take,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditLog>> GetByActorOrTargetUserIdAsync(
        string userId,
        int take,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

