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
        AuditInfo = null!;
    }

    private ServiceLevel(
        Guid id,
        string priority,
        TimeSpan responseTime,
        TimeSpan processingTime,
        bool useForSupport,
        bool useForIncidentManagement,
        AuditInfo auditInfo)
    {
        Id = id;
        Priority = priority;
        ResponseTime = responseTime;
        ProcessingTime = processingTime;
        UseForSupport = useForSupport;
        UseForIncidentManagement = useForIncidentManagement;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public string Priority { get; private set; }
    public TimeSpan ResponseTime { get; private set; }
    public TimeSpan ProcessingTime { get; private set; }
    public bool UseForSupport { get; private set; }
    public bool UseForIncidentManagement { get; private set; }
    public AuditInfo AuditInfo { get; private set; }

    public static ServiceLevel Create(
        string priority,
        TimeSpan responseTime,
        TimeSpan processingTime,
        bool useForSupport,
        bool useForIncidentManagement,
        ChangeContext changeContext)
    {
        ValidateTimes(responseTime, processingTime);
        ValidateUsage(useForSupport, useForIncidentManagement);

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
            auditInfo);
    }

    public void UpdateDetails(
        string priority,
        TimeSpan responseTime,
        TimeSpan processingTime,
        bool useForSupport,
        bool useForIncidentManagement,
        ChangeContext changeContext)
    {
        ValidateTimes(responseTime, processingTime);
        ValidateUsage(useForSupport, useForIncidentManagement);

        Priority = NormalizeRequired(priority, nameof(Priority));
        ResponseTime = responseTime;
        ProcessingTime = processingTime;
        UseForSupport = useForSupport;
        UseForIncidentManagement = useForIncidentManagement;

        Touch(changeContext);
    }

    private static string NormalizeRequired(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException($"{fieldName} is required.");
        }

        return value.Trim();
    }

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
