using SWFC.Domain.M100_System.M101_Foundation.Exceptions;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;

namespace SWFC.Domain.M700_Support.M704_Incident_Management;

public sealed class Incident
{
    private Incident()
    {
        Id = Guid.Empty;
        Category = IncidentCategory.PlantShutdown;
        Description = string.Empty;
        Escalation = string.Empty;
        ReactionControl = string.Empty;
        Status = IncidentStatus.Open;
        ModuleReference = null;
        ObjectReference = null;
        HistoryLog = string.Empty;
        NotificationReference = null;
        RuntimeReference = null;
        AuditInfo = null!;
    }

    private Incident(
        Guid id,
        IncidentCategory category,
        string description,
        string escalation,
        string reactionControl,
        IncidentStatus status,
        string? moduleReference,
        string? objectReference,
        string historyLog,
        string? notificationReference,
        string? runtimeReference,
        AuditInfo auditInfo)
    {
        Id = id;
        Category = category;
        Description = description;
        Escalation = escalation;
        ReactionControl = reactionControl;
        Status = status;
        ModuleReference = moduleReference;
        ObjectReference = objectReference;
        HistoryLog = historyLog;
        NotificationReference = notificationReference;
        RuntimeReference = runtimeReference;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public IncidentCategory Category { get; private set; }
    public string Description { get; private set; }
    public string Escalation { get; private set; }
    public string ReactionControl { get; private set; }
    public IncidentStatus Status { get; private set; }
    public string? ModuleReference { get; private set; }
    public string? ObjectReference { get; private set; }
    public string HistoryLog { get; private set; }
    public string? NotificationReference { get; private set; }
    public string? RuntimeReference { get; private set; }
    public AuditInfo AuditInfo { get; private set; }

    public static Incident Create(
        IncidentCategory category,
        string description,
        string escalation,
        string reactionControl,
        string? notificationReference,
        string? runtimeReference,
        ChangeContext changeContext,
        IncidentStatus status = IncidentStatus.Open,
        string? moduleReference = null,
        string? objectReference = null)
    {
        ValidateCategory(category);
        ValidateStatus(status);

        var auditInfo = new AuditInfo(
            createdAtUtc: changeContext.ChangedAtUtc,
            createdBy: changeContext.UserId);

        return new Incident(
            Guid.NewGuid(),
            category,
            NormalizeRequired(description, nameof(Description)),
            NormalizeRequired(escalation, nameof(Escalation)),
            NormalizeRequired(reactionControl, nameof(ReactionControl)),
            status,
            NormalizeOptional(moduleReference),
            NormalizeOptional(objectReference),
            CreateHistory("Created", status.ToString(), changeContext),
            NormalizeOptional(notificationReference),
            NormalizeOptional(runtimeReference),
            auditInfo);
    }

    public void UpdateDetails(
        IncidentCategory category,
        string description,
        string escalation,
        string reactionControl,
        string? notificationReference,
        string? runtimeReference,
        ChangeContext changeContext,
        IncidentStatus status = IncidentStatus.Open,
        string? moduleReference = null,
        string? objectReference = null)
    {
        ValidateCategory(category);
        ValidateStatus(status);

        Category = category;
        Description = NormalizeRequired(description, nameof(Description));
        Escalation = NormalizeRequired(escalation, nameof(Escalation));
        ReactionControl = NormalizeRequired(reactionControl, nameof(ReactionControl));
        Status = status;
        ModuleReference = NormalizeOptional(moduleReference) ?? ModuleReference;
        ObjectReference = NormalizeOptional(objectReference) ?? ObjectReference;
        HistoryLog = AppendHistory(HistoryLog, "Updated", status.ToString(), changeContext);
        NotificationReference = NormalizeOptional(notificationReference);
        RuntimeReference = NormalizeOptional(runtimeReference);

        Touch(changeContext);
    }

    private static void ValidateStatus(IncidentStatus status)
    {
        if (!Enum.IsDefined(status))
        {
            throw new ValidationException("Incident status is invalid.");
        }
    }

    private static string CreateHistory(string action, string value, ChangeContext changeContext) =>
        $"{changeContext.ChangedAtUtc:O}|{changeContext.UserId}|{action}|{value}|{changeContext.Reason}";

    private static string AppendHistory(string historyLog, string action, string value, ChangeContext changeContext) =>
        string.IsNullOrWhiteSpace(historyLog)
            ? CreateHistory(action, value, changeContext)
            : $"{historyLog}{Environment.NewLine}{CreateHistory(action, value, changeContext)}";

    private static string NormalizeRequired(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException($"{fieldName} is required.");
        }

        return value.Trim();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static void ValidateCategory(IncidentCategory category)
    {
        if (!Enum.IsDefined(category))
        {
            throw new ValidationException("Incident category is invalid.");
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

public enum IncidentCategory
{
    PlantShutdown = 0,
    SystemOutage = 1,
    SecurityIncident = 2
}

public enum IncidentStatus
{
    Open = 0,
    Escalated = 1,
    Contained = 2,
    Resolved = 3,
    Closed = 4
}
