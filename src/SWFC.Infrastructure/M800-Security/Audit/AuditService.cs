using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.M800_Security.M805_AuditCompliance.Entities;

namespace SWFC.Infrastructure.M800_Security.Audit;

public sealed class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditLogRepository;

    public AuditService(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task WriteAsync(
        string userId,
        string username,
        string action,
        string entity,
        string entityId,
        DateTime timestampUtc,
        string? oldValues = null,
        string? newValues = null,
        CancellationToken cancellationToken = default)
    {
        var auditLog = AuditLog.Create(
            userId,
            username,
            action,
            entity,
            entityId,
            timestampUtc,
            oldValues,
            newValues);

        await _auditLogRepository.AddAsync(auditLog, cancellationToken);
    }
}
