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
        string? notificationReference,
        string? runtimeReference,
        AuditInfo auditInfo)
    {
        Id = id;
        Category = category;
        Description = description;
        Escalation = escalation;
        ReactionControl = reactionControl;
        NotificationReference = notificationReference;
        RuntimeReference = runtimeReference;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public IncidentCategory Category { get; private set; }
    public string Description { get; private set; }
    public string Escalation { get; private set; }
    public string ReactionControl { get; private set; }
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
        ChangeContext changeContext)
    {
        ValidateCategory(category);

        var auditInfo = new AuditInfo(
            createdAtUtc: changeContext.ChangedAtUtc,
            createdBy: changeContext.UserId);

        return new Incident(
            Guid.NewGuid(),
            category,
            NormalizeRequired(description, nameof(Description)),
            NormalizeRequired(escalation, nameof(Escalation)),
            NormalizeRequired(reactionControl, nameof(ReactionControl)),
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
        ChangeContext changeContext)
    {
        ValidateCategory(category);

        Category = category;
        Description = NormalizeRequired(description, nameof(Description));
        Escalation = NormalizeRequired(escalation, nameof(Escalation));
        ReactionControl = NormalizeRequired(reactionControl, nameof(ReactionControl));
        NotificationReference = NormalizeOptional(notificationReference);
        RuntimeReference = NormalizeOptional(runtimeReference);

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
