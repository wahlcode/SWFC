namespace SWFC.Domain.M200_Business.M208_Safety;

public sealed class SafetyAssessment
{
    private SafetyAssessment()
    {
        Activity = string.Empty;
        Hazard = string.Empty;
        RequiredMeasures = string.Empty;
    }

    public SafetyAssessment(
        Guid id,
        string activity,
        SafetyTargetType targetType,
        Guid targetId,
        string hazard,
        int likelihood,
        int severity,
        string requiredMeasures,
        Guid? responsibleUserId,
        string? documentReference,
        Guid? qualityCaseId)
    {
        if (targetId == Guid.Empty)
        {
            throw new ArgumentException("Safety target is required.", nameof(targetId));
        }

        if (likelihood is < 1 or > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(likelihood), "Likelihood must be between 1 and 5.");
        }

        if (severity is < 1 or > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(severity), "Severity must be between 1 and 5.");
        }

        Id = id;
        Activity = RequireText(activity, nameof(activity));
        TargetType = targetType;
        TargetId = targetId;
        Hazard = RequireText(hazard, nameof(hazard));
        Likelihood = likelihood;
        Severity = severity;
        RequiredMeasures = RequireText(requiredMeasures, nameof(requiredMeasures));
        ResponsibleUserId = responsibleUserId;
        DocumentReference = NormalizeOptional(documentReference);
        QualityCaseId = qualityCaseId;
        Status = SafetyAssessmentStatus.Active;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Activity { get; private set; }
    public SafetyTargetType TargetType { get; private set; }
    public Guid TargetId { get; private set; }
    public string Hazard { get; private set; }
    public int Likelihood { get; private set; }
    public int Severity { get; private set; }
    public int RiskScore => Likelihood * Severity;
    public string RequiredMeasures { get; private set; }
    public Guid? ResponsibleUserId { get; private set; }
    public string? DocumentReference { get; private set; }
    public Guid? QualityCaseId { get; private set; }
    public SafetyAssessmentStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ClosedAtUtc { get; private set; }

    public void Close()
    {
        Status = SafetyAssessmentStatus.Closed;
        ClosedAtUtc = DateTime.UtcNow;
    }

    private static string RequireText(string value, string name)
        => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class SafetyPermit
{
    private SafetyPermit()
    {
        Activity = string.Empty;
        Restriction = string.Empty;
    }

    public SafetyPermit(Guid id, Guid assessmentId, string activity, DateTime validUntilUtc, string? restriction, Guid? approvedByUserId)
    {
        if (assessmentId == Guid.Empty)
        {
            throw new ArgumentException("Safety assessment is required.", nameof(assessmentId));
        }

        Id = id;
        AssessmentId = assessmentId;
        Activity = string.IsNullOrWhiteSpace(activity) ? throw new ArgumentException("Activity is required.", nameof(activity)) : activity.Trim();
        ValidUntilUtc = validUntilUtc;
        Restriction = string.IsNullOrWhiteSpace(restriction) ? string.Empty : restriction.Trim();
        ApprovedByUserId = approvedByUserId;
        Status = validUntilUtc > DateTime.UtcNow ? SafetyPermitStatus.Approved : SafetyPermitStatus.Expired;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid AssessmentId { get; private set; }
    public string Activity { get; private set; }
    public DateTime ValidUntilUtc { get; private set; }
    public string Restriction { get; private set; }
    public Guid? ApprovedByUserId { get; private set; }
    public SafetyPermitStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public bool AllowsAction(DateTime nowUtc)
        => Status == SafetyPermitStatus.Approved && ValidUntilUtc >= nowUtc;
}

public enum SafetyTargetType
{
    Machine = 1,
    Component = 2,
    Asset = 3,
    WorkActivity = 4
}

public enum SafetyAssessmentStatus
{
    Active = 1,
    Closed = 2
}

public enum SafetyPermitStatus
{
    Approved = 1,
    Expired = 2,
    Revoked = 3
}
