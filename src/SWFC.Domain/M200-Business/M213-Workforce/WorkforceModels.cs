namespace SWFC.Domain.M200_Business.M213_Workforce;

public enum WorkforceAssignmentStatus
{
    Planned,
    Active,
    Completed,
    Archived
}

public sealed record WorkforceTarget(string ModuleCode, Guid ObjectId, string Label)
{
    public bool IsLinked => !string.IsNullOrWhiteSpace(ModuleCode) && ObjectId != Guid.Empty;
}

public sealed record ActivityFeedback(DateTimeOffset StartedAtUtc, DateTimeOffset EndedAtUtc, string ResultNote)
{
    public TimeSpan Duration => EndedAtUtc - StartedAtUtc;
}

public sealed class WorkforceAssignment
{
    private readonly List<ActivityFeedback> _feedback = new();

    public WorkforceAssignment(
        Guid id,
        Guid userId,
        Guid? shiftModelId,
        WorkforceTarget target,
        DateTimeOffset plannedStartUtc,
        DateTimeOffset plannedEndUtc,
        string activity)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        UserId = userId == Guid.Empty ? throw new ArgumentException("User is required.", nameof(userId)) : userId;
        ShiftModelId = shiftModelId;
        Target = target.IsLinked ? target : throw new ArgumentException("Target must be linked.", nameof(target));
        PlannedStartUtc = plannedStartUtc;
        PlannedEndUtc = plannedEndUtc > plannedStartUtc ? plannedEndUtc : throw new ArgumentException("End must be after start.", nameof(plannedEndUtc));
        Activity = RequireText(activity, nameof(activity));
        Status = WorkforceAssignmentStatus.Planned;
    }

    public Guid Id { get; }
    public Guid UserId { get; }
    public Guid? ShiftModelId { get; }
    public WorkforceTarget Target { get; }
    public DateTimeOffset PlannedStartUtc { get; }
    public DateTimeOffset PlannedEndUtc { get; }
    public string Activity { get; }
    public WorkforceAssignmentStatus Status { get; private set; }
    public IReadOnlyList<ActivityFeedback> Feedback => _feedback;

    public void Start()
    {
        Status = WorkforceAssignmentStatus.Active;
    }

    public void AddFeedback(ActivityFeedback feedback)
    {
        if (feedback.Duration <= TimeSpan.Zero)
        {
            throw new ArgumentException("Feedback duration must be positive.", nameof(feedback));
        }

        _feedback.Add(feedback);
        Status = WorkforceAssignmentStatus.Completed;
    }

    public void Archive()
    {
        Status = WorkforceAssignmentStatus.Archived;
    }

    private static string RequireText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", parameterName);
        }

        return value.Trim();
    }
}
