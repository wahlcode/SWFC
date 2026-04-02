using SWFC.Domain.M800_Security.M805_AuditCompliance.Entities;

namespace SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
}