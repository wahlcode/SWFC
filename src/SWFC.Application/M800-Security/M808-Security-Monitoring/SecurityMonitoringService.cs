using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;

namespace SWFC.Application.M800_Security.M808_SecurityMonitoring;

public enum SecurityEventKind
{
    LoginSucceeded = 1,
    LoginFailed = 2,
    AccessDenied = 3,
    SecretAccessed = 4,
    PolicyViolation = 5,
    ApiRequest = 6
}

public enum SecurityAlertSeverity
{
    Info = 1,
    Warning = 2,
    Critical = 3
}

public sealed record SecurityMonitoringEvent(
    SecurityEventKind Kind,
    string ActorUserId,
    string Source,
    DateTime TimestampUtc,
    string ObjectType,
    string ObjectId,
    string? Detail = null);

public sealed record SecurityAlert(
    SecurityAlertSeverity Severity,
    string Code,
    string Message,
    SecurityMonitoringEvent TriggeredBy);

public interface ISecurityMonitoringService
{
    Task<IReadOnlyList<SecurityAlert>> AnalyzeAsync(
        IReadOnlyCollection<SecurityMonitoringEvent> events,
        CancellationToken cancellationToken = default);
}

public sealed class SecurityMonitoringService : ISecurityMonitoringService
{
    private readonly IAuditService _auditService;

    public SecurityMonitoringService(IAuditService auditService)
    {
        _auditService = auditService;
    }

    public async Task<IReadOnlyList<SecurityAlert>> AnalyzeAsync(
        IReadOnlyCollection<SecurityMonitoringEvent> events,
        CancellationToken cancellationToken = default)
    {
        var alerts = new List<SecurityAlert>();

        alerts.AddRange(DetectRepeatedFailedLogins(events));
        alerts.AddRange(DetectRepeatedDeniedAccess(events));
        alerts.AddRange(DetectSecretAccessWithoutReason(events));
        alerts.AddRange(DetectPolicyViolations(events));

        foreach (var alert in alerts)
        {
            await _auditService.WriteAsync(
                new AuditWriteRequest(
                    ActorUserId: string.IsNullOrWhiteSpace(alert.TriggeredBy.ActorUserId)
                        ? "system"
                        : alert.TriggeredBy.ActorUserId,
                    ActorDisplayName: "Security Monitoring",
                    Action: "SecurityAlertRaised",
                    Module: "M808",
                    ObjectType: alert.TriggeredBy.ObjectType,
                    ObjectId: alert.TriggeredBy.ObjectId,
                    TimestampUtc: DateTime.UtcNow,
                    NewValues: $"Severity={alert.Severity}; Code={alert.Code}; Source={alert.TriggeredBy.Source}",
                    Reason: alert.Message),
                cancellationToken);
        }

        return alerts;
    }

    private static IEnumerable<SecurityAlert> DetectRepeatedFailedLogins(
        IEnumerable<SecurityMonitoringEvent> events)
    {
        return events
            .Where(item => item.Kind == SecurityEventKind.LoginFailed)
            .GroupBy(item => new { item.ActorUserId, item.Source })
            .Where(group => group.Count() >= 5)
            .Select(group => new SecurityAlert(
                SecurityAlertSeverity.Critical,
                "m808.login.failed.repeated",
                "Repeated failed logins detected.",
                group.OrderByDescending(item => item.TimestampUtc).First()));
    }

    private static IEnumerable<SecurityAlert> DetectRepeatedDeniedAccess(
        IEnumerable<SecurityMonitoringEvent> events)
    {
        return events
            .Where(item => item.Kind == SecurityEventKind.AccessDenied)
            .GroupBy(item => item.ActorUserId)
            .Where(group => group.Count() >= 3)
            .Select(group => new SecurityAlert(
                SecurityAlertSeverity.Warning,
                "m808.access.denied.repeated",
                "Repeated denied access decisions detected.",
                group.OrderByDescending(item => item.TimestampUtc).First()));
    }

    private static IEnumerable<SecurityAlert> DetectSecretAccessWithoutReason(
        IEnumerable<SecurityMonitoringEvent> events)
    {
        return events
            .Where(item => item.Kind == SecurityEventKind.SecretAccessed)
            .Where(item => string.IsNullOrWhiteSpace(item.Detail))
            .Select(item => new SecurityAlert(
                SecurityAlertSeverity.Critical,
                "m808.secret.reason_missing",
                "Secret access without a reason detected.",
                item));
    }

    private static IEnumerable<SecurityAlert> DetectPolicyViolations(
        IEnumerable<SecurityMonitoringEvent> events)
    {
        return events
            .Where(item => item.Kind == SecurityEventKind.PolicyViolation)
            .Select(item => new SecurityAlert(
                SecurityAlertSeverity.Critical,
                "m808.policy.violation",
                "Security policy violation detected.",
                item));
    }
}
