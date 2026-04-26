namespace SWFC.Domain.M200_Business.M209_Projects;

public enum ProjectStatus
{
    Planned,
    InProgress,
    Paused,
    Completed,
    Archived
}

public sealed record ProjectReference(string ModuleCode, Guid? ObjectId, string Label)
{
    public bool IsLinked => !string.IsNullOrWhiteSpace(ModuleCode) && !string.IsNullOrWhiteSpace(Label);
}

public sealed record ProjectMaterialReference(Guid InventoryItemId, decimal Quantity, string Unit)
{
    public bool UsesInventoryOnly => InventoryItemId != Guid.Empty && Quantity > 0 && !string.IsNullOrWhiteSpace(Unit);
}

public sealed class ProjectTask
{
    public ProjectTask(Guid id, string title, Guid? responsibleUserId, DateOnly? dueDate)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        Title = RequireText(title, nameof(title));
        ResponsibleUserId = responsibleUserId;
        DueDate = dueDate;
        Status = ProjectStatus.Planned;
    }

    public Guid Id { get; }
    public string Title { get; private set; }
    public Guid? ResponsibleUserId { get; private set; }
    public DateOnly? DueDate { get; private set; }
    public ProjectStatus Status { get; private set; }

    public void Complete()
    {
        Status = ProjectStatus.Completed;
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

public sealed class ProjectMeasure
{
    private readonly List<ProjectTask> _tasks = new();

    public ProjectMeasure(Guid id, string title, Guid? responsibleUserId, DateOnly plannedStart, DateOnly? plannedEnd)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        Title = RequireText(title, nameof(title));
        ResponsibleUserId = responsibleUserId;
        PlannedStart = plannedStart;
        PlannedEnd = plannedEnd;
        Status = ProjectStatus.Planned;
    }

    public Guid Id { get; }
    public string Title { get; private set; }
    public Guid? ResponsibleUserId { get; private set; }
    public DateOnly PlannedStart { get; private set; }
    public DateOnly? PlannedEnd { get; private set; }
    public ProjectStatus Status { get; private set; }
    public IReadOnlyList<ProjectTask> Tasks => _tasks;

    public ProjectTask AddTask(string title, Guid? responsibleUserId, DateOnly? dueDate)
    {
        var task = new ProjectTask(Guid.NewGuid(), title, responsibleUserId, dueDate);
        _tasks.Add(task);
        return task;
    }

    public void Start()
    {
        Status = ProjectStatus.InProgress;
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

public sealed class Project
{
    private readonly List<ProjectMeasure> _measures = new();
    private readonly List<ProjectReference> _references = new();
    private readonly List<ProjectMaterialReference> _materials = new();

    public Project(Guid id, string title, Guid organizationUnitId, DateOnly plannedStart, DateOnly? plannedEnd)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        Title = RequireText(title, nameof(title));
        OrganizationUnitId = organizationUnitId == Guid.Empty
            ? throw new ArgumentException("Organization unit is required.", nameof(organizationUnitId))
            : organizationUnitId;
        PlannedStart = plannedStart;
        PlannedEnd = plannedEnd;
        Status = ProjectStatus.Planned;
    }

    public Guid Id { get; }
    public string Title { get; private set; }
    public Guid OrganizationUnitId { get; }
    public DateOnly PlannedStart { get; private set; }
    public DateOnly? PlannedEnd { get; private set; }
    public ProjectStatus Status { get; private set; }
    public IReadOnlyList<ProjectMeasure> Measures => _measures;
    public IReadOnlyList<ProjectReference> References => _references;
    public IReadOnlyList<ProjectMaterialReference> Materials => _materials;

    public ProjectMeasure AddMeasure(string title, Guid? responsibleUserId, DateOnly plannedStart, DateOnly? plannedEnd)
    {
        var measure = new ProjectMeasure(Guid.NewGuid(), title, responsibleUserId, plannedStart, plannedEnd);
        _measures.Add(measure);
        return measure;
    }

    public void LinkReference(ProjectReference reference)
    {
        if (!reference.IsLinked)
        {
            throw new ArgumentException("Project reference must include module and label.", nameof(reference));
        }

        _references.Add(reference);
    }

    public void AddMaterial(ProjectMaterialReference material)
    {
        if (!material.UsesInventoryOnly)
        {
            throw new ArgumentException("Project material must reference M204 inventory.", nameof(material));
        }

        _materials.Add(material);
    }

    public void Start()
    {
        Status = ProjectStatus.InProgress;
    }

    public void Archive()
    {
        Status = ProjectStatus.Archived;
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
