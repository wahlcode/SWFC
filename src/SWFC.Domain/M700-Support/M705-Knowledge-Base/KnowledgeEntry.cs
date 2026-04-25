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
        AuditInfo = null!;
    }

    private KnowledgeEntry(
        Guid id,
        KnowledgeEntryType type,
        string content,
        AuditInfo auditInfo)
    {
        Id = id;
        Type = type;
        Content = content;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public KnowledgeEntryType Type { get; private set; }
    public string Content { get; private set; }
    public AuditInfo AuditInfo { get; private set; }

    public static KnowledgeEntry Create(
        KnowledgeEntryType type,
        string content,
        ChangeContext changeContext)
    {
        ValidateType(type);

        var auditInfo = new AuditInfo(
            createdAtUtc: changeContext.ChangedAtUtc,
            createdBy: changeContext.UserId);

        return new KnowledgeEntry(
            Guid.NewGuid(),
            type,
            NormalizeRequired(content, nameof(Content)),
            auditInfo);
    }

    public void UpdateDetails(
        KnowledgeEntryType type,
        string content,
        ChangeContext changeContext)
    {
        ValidateType(type);

        Type = type;
        Content = NormalizeRequired(content, nameof(Content));

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
