using SWFC.Domain.M700_Support.M705_Knowledge_Base;

namespace SWFC.Application.M700_Support.M705_Knowledge_Base;

public sealed record KnowledgeEntryListItem(
    Guid Id,
    KnowledgeEntryType Type,
    string Content,
    DateTime CreatedAtUtc,
    string CreatedBy,
    DateTime? LastModifiedAtUtc,
    string? LastModifiedBy);
