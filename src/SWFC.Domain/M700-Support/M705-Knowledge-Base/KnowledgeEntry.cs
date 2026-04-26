using SWFC.Domain.M100_System.M101_Foundation.Exceptions;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;

namespace SWFC.Domain.M700_Support.M705_Knowledge_Base;

public sealed class KnowledgeEntry
{
    private KnowledgeEntry()
    {
        Id = Guid.Empty;
        Type = KnowledgeEntryType.ProblemSolution;
        Content = string.Empty;
        Status = KnowledgeEntryStatus.Draft;
        ModuleReference = null;
        ObjectReference = null;
        HistoryLog = string.Empty;
        AuditInfo = null!;
    }

    private KnowledgeEntry(
        Guid id,
        KnowledgeEntryType type,
        string content,
        KnowledgeEntryStatus status,
        string? moduleReference,
        string? objectReference,
        string historyLog,
        AuditInfo auditInfo)
    {
        Id = id;
        Type = type;
        Content = content;
        Status = status;
        ModuleReference = moduleReference;
        ObjectReference = objectReference;
        HistoryLog = historyLog;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public KnowledgeEntryType Type { get; private set; }
    public string Content { get; private set; }
    public KnowledgeEntryStatus Status { get; private set; }
    public string? ModuleReference { get; private set; }
    public string? ObjectReference { get; private set; }
    public string HistoryLog { get; private set; }
    public AuditInfo AuditInfo { get; private set; }

    public static KnowledgeEntry Create(
        KnowledgeEntryType type,
        string content,
        ChangeContext changeContext,
        KnowledgeEntryStatus status = KnowledgeEntryStatus.Draft,
        string? moduleReference = null,
        string? objectReference = null)
    {
        ValidateType(type);
        ValidateStatus(status);

        var auditInfo = new AuditInfo(
            createdAtUtc: changeContext.ChangedAtUtc,
            createdBy: changeContext.UserId);

        return new KnowledgeEntry(
            Guid.NewGuid(),
            type,
            NormalizeRequired(content, nameof(Content)),
            status,
            NormalizeOptional(moduleReference),
            NormalizeOptional(objectReference),
            CreateHistory("Created", status.ToString(), changeContext),
            auditInfo);
    }

    public void UpdateDetails(
        KnowledgeEntryType type,
        string content,
        ChangeContext changeContext,
        KnowledgeEntryStatus status = KnowledgeEntryStatus.Draft,
        string? moduleReference = null,
        string? objectReference = null)
    {
        ValidateType(type);
        ValidateStatus(status);

        Type = type;
        Content = NormalizeRequired(content, nameof(Content));
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

    private static void ValidateStatus(KnowledgeEntryStatus status)
    {
        if (!Enum.IsDefined(status))
        {
            throw new ValidationException("Knowledge entry status is invalid.");
        }
    }

    private static string CreateHistory(string action, string value, ChangeContext changeContext) =>
        $"{changeContext.ChangedAtUtc:O}|{changeContext.UserId}|{action}|{value}|{changeContext.Reason}";

    private static string AppendHistory(string historyLog, string action, string value, ChangeContext changeContext) =>
        string.IsNullOrWhiteSpace(historyLog)
            ? CreateHistory(action, value, changeContext)
            : $"{historyLog}{Environment.NewLine}{CreateHistory(action, value, changeContext)}";

    private static void ValidateType(KnowledgeEntryType type)
    {
        if (!Enum.IsDefined(type))
        {
            throw new ValidationException("Knowledge entry type is invalid.");
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

public enum KnowledgeEntryType
{
    ProblemSolution = 0,
    Instruction = 1,
    BestPractice = 2
}

public enum KnowledgeEntryStatus
{
    Draft = 0,
    Reviewed = 1,
    Published = 2,
    Archived = 3
}
