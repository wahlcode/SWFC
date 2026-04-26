using SWFC.Domain.M100_System.M101_Foundation.Exceptions;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;

namespace SWFC.Domain.M700_Support.M706_SLA_Service_Levels;

public sealed class ServiceLevel
{
    private ServiceLevel()
    {
        Id = Guid.Empty;
        Priority = string.Empty;
        ResponseTime = TimeSpan.Zero;
        ProcessingTime = TimeSpan.Zero;
        UseForSupport = false;
        UseForIncidentManagement = false;
        Status = ServiceLevelStatus.Draft;
        ModuleReference = null;
        ObjectReference = null;
        HistoryLog = string.Empty;
        AuditInfo = null!;
    }

    private ServiceLevel(
        Guid id,
        string priority,
        TimeSpan responseTime,
        TimeSpan processingTime,
        bool useForSupport,
        bool useForIncidentManagement,
        ServiceLevelStatus status,
        string? moduleReference,
        string? objectReference,
        string historyLog,
        AuditInfo auditInfo)
    {
        Id = id;
        Priority = priority;
        ResponseTime = responseTime;
        ProcessingTime = processingTime;
        UseForSupport = useForSupport;
        UseForIncidentManagement = useForIncidentManagement;
        Status = status;
        ModuleReference = moduleReference;
        ObjectReference = objectReference;
        HistoryLog = historyLog;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public string Priority { get; private set; }
    public TimeSpan ResponseTime { get; private set; }
    public TimeSpan ProcessingTime { get; private set; }
    public bool UseForSupport { get; private set; }
    public bool UseForIncidentManagement { get; private set; }
    public ServiceLevelStatus Status { get; private set; }
    public string? ModuleReference { get; private set; }
    public string? ObjectReference { get; private set; }
    public string HistoryLog { get; private set; }
    public AuditInfo AuditInfo { get; private set; }

    public static ServiceLevel Create(
        string priority,
        TimeSpan responseTime,
        TimeSpan processingTime,
        bool useForSupport,
        bool useForIncidentManagement,
        ChangeContext changeContext,
        ServiceLevelStatus status = ServiceLevelStatus.Draft,
        string? moduleReference = null,
        string? objectReference = null)
    {
        ValidateTimes(responseTime, processingTime);
        ValidateUsage(useForSupport, useForIncidentManagement);
        ValidateStatus(status);

        var auditInfo = new AuditInfo(
            createdAtUtc: changeContext.ChangedAtUtc,
            createdBy: changeContext.UserId);

        return new ServiceLevel(
            Guid.NewGuid(),
            NormalizeRequired(priority, nameof(Priority)),
            responseTime,
            processingTime,
            useForSupport,
            useForIncidentManagement,
            status,
            NormalizeOptional(moduleReference),
            NormalizeOptional(objectReference),
            CreateHistory("Created", status.ToString(), changeContext),
            auditInfo);
    }

    public void UpdateDetails(
        string priority,
        TimeSpan responseTime,
        TimeSpan processingTime,
        bool useForSupport,
        bool useForIncidentManagement,
        ChangeContext changeContext,
        ServiceLevelStatus status = ServiceLevelStatus.Draft,
        string? moduleReference = null,
        string? objectReference = null)
    {
        ValidateTimes(responseTime, processingTime);
        ValidateUsage(useForSupport, useForIncidentManagement);
        ValidateStatus(status);

        Priority = NormalizeRequired(priority, nameof(Priority));
        ResponseTime = responseTime;
        ProcessingTime = processingTime;
        UseForSupport = useForSupport;
        UseForIncidentManagement = useForIncidentManagement;
        Status = status;
        ModuleReference = NormalizeOptional(moduleReference) ?? ModuleReference;
        ObjectReference = NormalizeOptional(objectReference) ?? ObjectReference;
        HistoryLog = AppendHistory(HistoryLog, "Updated", status.ToString(), changeContext);

        Touch(changeContext);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string NormalizeRequired(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException($"{fieldName} is required.");
        }

        return value.Trim();
    }

    private static void ValidateStatus(ServiceLevelStatus status)
    {
        if (!Enum.IsDefined(status))
        {
            throw new ValidationException("Service level status is invalid.");
        }
    }

    private static string CreateHistory(string action, string value, ChangeContext changeContext) =>
        $"{changeContext.ChangedAtUtc:O}|{changeContext.UserId}|{action}|{value}|{changeContext.Reason}";

    private static string AppendHistory(string historyLog, string action, string value, ChangeContext changeContext) =>
        string.IsNullOrWhiteSpace(historyLog)
            ? CreateHistory(action, value, changeContext)
            : $"{historyLog}{Environment.NewLine}{CreateHistory(action, value, changeContext)}";

    private static void ValidateTimes(TimeSpan responseTime, TimeSpan processingTime)
    {
        if (responseTime <= TimeSpan.Zero || processingTime <= TimeSpan.Zero)
        {
            throw new ValidationException("Service times must be greater than zero.");
        }
    }

    private static void ValidateUsage(bool useForSupport, bool useForIncidentManagement)
    {
        if (!useForSupport && !useForIncidentManagement)
        {
            throw new ValidationException("At least one service level usage must be selected.");
        }
    }

    private void Touch(ChangeContext changeContext)
    {
        AuditInfo = new AuditInfo(
            createdAtUtc: AuditInfo.CreatedAtUtc,
            createdBy: AuditInfo.CreatedBy,
            lastModifiedAtUtc: changeContext.ChangedAtUtc,
            lastModifiedBy: changeContext.UserId);
    }
}

public enum ServiceLevelStatus
{
    Draft = 0,
    Active = 1,
    Retired = 2
}
