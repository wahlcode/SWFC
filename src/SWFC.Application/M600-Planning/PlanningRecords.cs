using SWFC.Domain.M100_System.M101_Foundation.Exceptions;

namespace SWFC.Application.M600_Planning;

public sealed class PlanningRecord
{
    private readonly List<PlanningLink> _links = new();
    private readonly List<PlanningChange> _changes = new();

    private PlanningRecord(
        string moduleCode,
        string title,
        string description,
        PlanningRecordKind kind,
        PlanningRecordStatus status,
        string owner,
        DateTime changedAtUtc,
        string reason)
    {
        Id = Guid.NewGuid();
        ModuleCode = NormalizeModule(moduleCode);
        Title = NormalizeRequired(title, nameof(Title));
        Description = NormalizeRequired(description, nameof(Description));
        Kind = kind;
        Status = status;
        Owner = NormalizeRequired(owner, nameof(Owner));
        CreatedAtUtc = changedAtUtc;
        LastChangedAtUtc = changedAtUtc;

        AddChange("Created", status.ToString(), owner, changedAtUtc, reason);
    }

    public Guid Id { get; }
    public string ModuleCode { get; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public PlanningRecordKind Kind { get; }
    public PlanningRecordStatus Status { get; private set; }
    public string Owner { get; private set; }
    public DateTime CreatedAtUtc { get; }
    public DateTime LastChangedAtUtc { get; private set; }
    public IReadOnlyList<PlanningLink> Links => _links;
    public IReadOnlyList<PlanningChange> Changes => _changes;

    public static PlanningRecord Create(
        string moduleCode,
        string title,
        string description,
        PlanningRecordKind kind,
        PlanningRecordStatus status,
        string owner,
        DateTime changedAtUtc,
        string reason)
    {
        ValidateEnum(kind, nameof(kind));
        ValidateEnum(status, nameof(status));

        return new PlanningRecord(moduleCode, title, description, kind, status, owner, changedAtUtc, reason);
    }

    public void UpdateDetails(
        string title,
        string description,
        string owner,
        DateTime changedAtUtc,
        string reason)
    {
        Title = NormalizeRequired(title, nameof(Title));
        Description = NormalizeRequired(description, nameof(Description));
        Owner = NormalizeRequired(owner, nameof(Owner));
        LastChangedAtUtc = changedAtUtc;

        AddChange("DetailsUpdated", Status.ToString(), owner, changedAtUtc, reason);
    }

    public void ChangeStatus(
        PlanningRecordStatus status,
        string changedBy,
        DateTime changedAtUtc,
        string reason)
    {
        ValidateEnum(status, nameof(status));

        Status = status;
        LastChangedAtUtc = changedAtUtc;

        AddChange("StatusChanged", status.ToString(), changedBy, changedAtUtc, reason);
    }

    public void LinkTo(
        string targetModuleCode,
        string targetReference,
        PlanningLinkType type,
        string changedBy,
        DateTime changedAtUtc,
        string reason)
    {
        ValidateEnum(type, nameof(type));

        var link = new PlanningLink(
            NormalizeModule(targetModuleCode),
            NormalizeRequired(targetReference, nameof(targetReference)),
            type);

        if (!_links.Contains(link))
        {
            _links.Add(link);
            LastChangedAtUtc = changedAtUtc;
            AddChange("Linked", $"{link.Type}:{link.TargetModuleCode}:{link.TargetReference}", changedBy, changedAtUtc, reason);
        }
    }

    private void AddChange(
        string action,
        string value,
        string changedBy,
        DateTime changedAtUtc,
        string reason)
    {
        _changes.Add(new PlanningChange(
            NormalizeRequired(action, nameof(action)),
            NormalizeRequired(value, nameof(value)),
            NormalizeRequired(changedBy, nameof(changedBy)),
            changedAtUtc,
            NormalizeRequired(reason, nameof(reason))));
    }

    private static string NormalizeModule(string value)
    {
        var normalized = NormalizeRequired(value, nameof(ModuleCode)).ToUpperInvariant();
        if (!normalized.StartsWith('M') || normalized.Length < 4)
        {
            throw new ValidationException("Module code is invalid.");
        }

        return normalized;
    }

    private static string NormalizeRequired(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException($"{fieldName} is required.");
        }

        return value.Trim();
    }

    private static void ValidateEnum<TEnum>(TEnum value, string fieldName)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new ValidationException($"{fieldName} is invalid.");
        }
    }
}

public class PlanningCatalog
{
    private readonly List<PlanningRecord> _records = new();

    public PlanningCatalog(string moduleCode, PlanningRecordKind allowedKind)
    {
        ModuleCode = moduleCode;
        AllowedKind = allowedKind;
    }

    public string ModuleCode { get; }
    public PlanningRecordKind AllowedKind { get; }
    public IReadOnlyList<PlanningRecord> Records => _records;

    public PlanningRecord Add(
        string title,
        string description,
        PlanningRecordStatus status,
        string owner,
        DateTime changedAtUtc,
        string reason)
    {
        var record = PlanningRecord.Create(
            ModuleCode,
            title,
            description,
            AllowedKind,
            status,
            owner,
            changedAtUtc,
            reason);

        _records.Add(record);
        return record;
    }

    public PlanningRecord Get(Guid id)
    {
        return _records.SingleOrDefault(x => x.Id == id)
            ?? throw new ValidationException($"Planning record '{id}' was not found.");
    }
}

public sealed record PlanningLink(
    string TargetModuleCode,
    string TargetReference,
    PlanningLinkType Type);

public sealed record PlanningChange(
    string Action,
    string Value,
    string ChangedBy,
    DateTime ChangedAtUtc,
    string Reason);

public enum PlanningRecordKind
{
    Roadmap = 0,
    Concept = 1,
    Prototype = 2,
    Decision = 3,
    Standard = 4,
    Requirement = 5,
    Evaluation = 6
}

public enum PlanningRecordStatus
{
    Draft = 0,
    InReview = 1,
    Accepted = 2,
    Rejected = 3,
    Superseded = 4
}

public enum PlanningLinkType
{
    RelatesTo = 0,
    DependsOn = 1,
    Evaluates = 2,
    Decides = 3,
    Schedules = 4
}
