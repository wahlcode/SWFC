namespace SWFC.Domain.M200_Business.M207_Quality;

public sealed class QualityCase
{
    private QualityCase()
    {
        Title = string.Empty;
        Description = string.Empty;
        SourceReference = string.Empty;
    }

    public QualityCase(
        Guid id,
        string title,
        string description,
        QualityCaseSource source,
        string? sourceReference,
        Guid? machineId,
        Guid? maintenanceOrderId,
        Guid? inspectionId,
        QualityPriority priority,
        DateTime? dueAtUtc,
        Guid? responsibleUserId)
    {
        Id = id;
        Title = RequireText(title, nameof(title));
        Description = RequireText(description, nameof(description));
        Source = source;
        SourceReference = NormalizeOptional(sourceReference);
        MachineId = machineId;
        MaintenanceOrderId = maintenanceOrderId;
        InspectionId = inspectionId;
        Priority = priority;
        DueAtUtc = dueAtUtc;
        ResponsibleUserId = responsibleUserId;
        Status = QualityCaseStatus.Open;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public QualityCaseSource Source { get; private set; }
    public string? SourceReference { get; private set; }
    public Guid? MachineId { get; private set; }
    public Guid? MaintenanceOrderId { get; private set; }
    public Guid? InspectionId { get; private set; }
    public QualityPriority Priority { get; private set; }
    public QualityCaseStatus Status { get; private set; }
    public string? RootCause { get; private set; }
    public DateTime? DueAtUtc { get; private set; }
    public Guid? ResponsibleUserId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ClosedAtUtc { get; private set; }

    public void StartRootCauseAnalysis(string rootCause)
    {
        RootCause = RequireText(rootCause, nameof(rootCause));
        Status = QualityCaseStatus.InProgress;
    }

    public void Resolve()
    {
        Status = QualityCaseStatus.Resolved;
    }

    public void Close()
    {
        Status = QualityCaseStatus.Closed;
        ClosedAtUtc = DateTime.UtcNow;
    }

    public int EscalationLevel(DateTime nowUtc)
    {
        if (!DueAtUtc.HasValue || Status is QualityCaseStatus.Resolved or QualityCaseStatus.Closed)
        {
            return 0;
        }

        return nowUtc > DueAtUtc.Value ? (int)Priority : 0;
    }

    private static string RequireText(string value, string name)
        => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class QualityAction
{
    private QualityAction()
    {
        Title = string.Empty;
    }

    public QualityAction(Guid id, Guid qualityCaseId, string title, QualityActionType type, Guid? assignedUserId, DateTime? dueAtUtc)
    {
        if (qualityCaseId == Guid.Empty)
        {
            throw new ArgumentException("Quality case is required.", nameof(qualityCaseId));
        }

        Id = id;
        QualityCaseId = qualityCaseId;
        Title = string.IsNullOrWhiteSpace(title) ? throw new ArgumentException("Title is required.", nameof(title)) : title.Trim();
        Type = type;
        Status = QualityActionStatus.Planned;
        AssignedUserId = assignedUserId;
        DueAtUtc = dueAtUtc;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid QualityCaseId { get; private set; }
    public string Title { get; private set; }
    public QualityActionType Type { get; private set; }
    public QualityActionStatus Status { get; private set; }
    public Guid? AssignedUserId { get; private set; }
    public DateTime? DueAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ClosedAtUtc { get; private set; }

    public void Complete()
    {
        Status = QualityActionStatus.Completed;
        ClosedAtUtc = DateTime.UtcNow;
    }
}

public enum QualityCaseSource
{
    Manual = 1,
    Inspection = 2,
    Maintenance = 3,
    Operations = 4,
    Safety = 5
}

public enum QualityPriority
{
    Low = 1,
    Normal = 2,
    High = 3,
    Critical = 4
}

public enum QualityCaseStatus
{
    Open = 1,
    InProgress = 2,
    Resolved = 3,
    Closed = 4
}

public enum QualityActionType
{
    Corrective = 1,
    Preventive = 2
}

public enum QualityActionStatus
{
    Planned = 1,
    Completed = 2
}
