using SWFC.Domain.M800_Security.M801_Access;

namespace SWFC.Application.M800_Security.M803_Visibility.AccessRules;

public sealed record AccessRuleListItemDto(
    Guid Id,
    AccessTargetType TargetType,
    string TargetId,
    AccessSubjectType SubjectType,
    string SubjectId,
    AccessRuleMode Mode,
    DateTime CreatedAtUtc,
    string CreatedBy,
    DateTime? LastModifiedAtUtc,
    string? LastModifiedBy);
