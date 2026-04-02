namespace SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;

public interface IAuditService
{
    Task WriteAsync(
        string userId,
        string username,
        string action,
        string entity,
        string entityId,
        DateTime timestampUtc,
        string? oldValues = null,
        string? newValues = null,
        CancellationToken cancellationToken = default);
}