namespace SWFC.Domain.M200_Business.M203_Inspections;

public sealed class InspectionPlan
{
    private InspectionPlan()
    {
        Name = string.Empty;
        ObjectType = string.Empty;
    }

    public InspectionPlan(
        Guid id,
        string name,
        InspectionTargetType targetType,
        Guid targetId,
        string objectType,
        int intervalDays,
        Guid? responsibleUserId,
        string? documentReference)
    {
        if (targetId == Guid.Empty)
        {
            throw new ArgumentException("Inspection target is required.", nameof(targetId));
        }

        if (intervalDays <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(intervalDays), "Inspection interval must be positive.");
        }

        Id = id;
        Name = RequireText(name, nameof(name));
        TargetType = targetType;
        TargetId = targetId;
        ObjectType = RequireText(objectType, nameof(objectType));
        IntervalDays = intervalDays;
        ResponsibleUserId = responsibleUserId;
        DocumentReference = NormalizeOptional(documentReference);
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
        NextDueAtUtc = CreatedAtUtc.Date.AddDays(intervalDays);
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public InspectionTargetType TargetType { get; private set; }
    public Guid TargetId { get; private set; }
    public string ObjectType { get; private set; }
    public int IntervalDays { get; private set; }
    public DateTime NextDueAtUtc { get; private set; }
    public Guid? ResponsibleUserId { get; private set; }
    public string? DocumentReference { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ClosedAtUtc { get; private set; }

    public void MarkCycleCompleted(DateTime completedAtUtc)
    {
        NextDueAtUtc = completedAtUtc.Date.AddDays(IntervalDays);
    }

    public void Close()
    {
        IsActive = false;
        ClosedAtUtc = DateTime.UtcNow;
    }

    private static string RequireText(string value, string name)
        => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class Inspection
{
    private Inspection()
    {
        Title = string.Empty;
        Notes = string.Empty;
    }

    public Inspection(
        Guid id,
        Guid inspectionPlanId,
        InspectionTargetType targetType,
        Guid targetId,
        string title,
        InspectionResult result,
        Guid? inspectorUserId,
        string? notes,
        string? followUpReference)
    {
        if (inspectionPlanId == Guid.Empty)
        {
            throw new ArgumentException("Inspection plan is required.", nameof(inspectionPlanId));
        }

        if (targetId == Guid.Empty)
        {
            throw new ArgumentException("Inspection target is required.", nameof(targetId));
        }

        Id = id;
        InspectionPlanId = inspectionPlanId;
        TargetType = targetType;
        TargetId = targetId;
        Title = RequireText(title, nameof(title));
        Result = result;
        Status = result == InspectionResult.Passed ? InspectionStatus.Released : InspectionStatus.ActionRequired;
        InspectorUserId = inspectorUserId;
        Notes = NormalizeOptional(notes) ?? string.Empty;
        FollowUpReference = NormalizeOptional(followUpReference);
        PerformedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid InspectionPlanId { get; private set; }
    public InspectionTargetType TargetType { get; private set; }
    public Guid TargetId { get; private set; }
    public string Title { get; private set; }
    public InspectionResult Result { get; private set; }
    public InspectionStatus Status { get; private set; }
    public Guid? InspectorUserId { get; private set; }
    public string Notes { get; private set; }
    public string? FollowUpReference { get; private set; }
    public DateTime PerformedAtUtc { get; private set; }

    private static string RequireText(string value, string name)
        => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public enum InspectionTargetType
{
    Machine = 1,
    Component = 2,
    Asset = 3
}

public enum InspectionResult
{
    Passed = 1,
    Failed = 2,
    DefectFound = 3,
    ReinspectionRequired = 4
}

public enum InspectionStatus
{
    Planned = 1,
    Released = 2,
    ActionRequired = 3,
    Closed = 4
}
