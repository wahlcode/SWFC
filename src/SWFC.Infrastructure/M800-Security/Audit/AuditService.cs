using Microsoft.AspNetCore.Http;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.M800_Security.M805_AuditCompliance.Entities;

namespace SWFC.Infrastructure.M800_Security.Audit;

public sealed class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(
        IAuditLogRepository auditLogRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _auditLogRepository = auditLogRepository;
        _httpContextAccessor = httpContextAccessor;
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
        await WriteAsync(
            new AuditWriteRequest(
                ActorUserId: userId,
                ActorDisplayName: username,
                Action: action,
                Module: "General",
                ObjectType: entity,
                ObjectId: entityId,
                TimestampUtc: timestampUtc,
                OldValues: oldValues,
                NewValues: newValues),
            cancellationToken);
    }

    public async Task WriteAsync(
        AuditWriteRequest request,
        CancellationToken cancellationToken = default)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var clientIp = request.ClientIp
            ?? httpContext?.Connection.RemoteIpAddress?.ToString();
        var clientInfo = request.ClientInfo
            ?? httpContext?.Request.Headers.UserAgent.ToString();

        var auditLog = AuditLog.Create(
            request.ActorUserId,
            request.ActorDisplayName,
            request.Action,
            request.Module,
            request.ObjectType,
            request.ObjectId,
            request.TimestampUtc,
            request.TargetUserId,
            request.OldValues,
            request.NewValues,
            clientIp,
            clientInfo,
            request.ApprovedByUserId,
            request.Reason);

        await _auditLogRepository.AddAsync(auditLog, cancellationToken);
        await _auditLogRepository.SaveChangesAsync(cancellationToken);
    }
}
