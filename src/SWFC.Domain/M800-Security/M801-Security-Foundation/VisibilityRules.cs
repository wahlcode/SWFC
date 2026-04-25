using SWFC.Domain.M100_System.M101_Foundation.Exceptions;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;

namespace SWFC.Domain.M800_Security.M801_Access;

public enum AccessTargetType
{
    Machine = 1,
    MachineComponentArea = 2,
    MachineComponent = 3
}

public enum AccessSubjectType
{
    User = 1,
    Role = 2,
    OrganizationUnit = 3
}

public enum AccessRuleMode
{
    Allow = 1,
    Deny = 2
}

public sealed class AccessRule
{
    private AccessRule()
    {
        Id = Guid.Empty;
        TargetId = null!;
        SubjectId = null!;
        AuditInfo = null!;
    }

    private AccessRule(
        Guid id,
        AccessTargetType targetType,
        string targetId,
        AccessSubjectType subjectType,
        string subjectId,
        AccessRuleMode mode,
        bool isActive,
        AuditInfo auditInfo)
    {
        Id = id;
        TargetType = targetType;
        TargetId = targetId;
        SubjectType = subjectType;
        SubjectId = subjectId;
        Mode = mode;
        IsActive = isActive;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public AccessTargetType TargetType { get; private set; }
    public string TargetId { get; private set; }
    public AccessSubjectType SubjectType { get; private set; }
    public string SubjectId { get; private set; }
    public AccessRuleMode Mode { get; private set; }
    public bool IsActive { get; private set; }
    public AuditInfo AuditInfo { get; private set; }

    public static AccessRule Create(
        AccessTargetType targetType,
        string targetId,
        AccessSubjectType subjectType,
        string subjectId,
        AccessRuleMode mode,
        ChangeContext changeContext)
    {
        var normalizedTargetId = NormalizeRequired(targetId, nameof(targetId));
        var normalizedSubjectId = NormalizeRequired(subjectId, nameof(subjectId));

        var auditInfo = new AuditInfo(
            createdAtUtc: changeContext.ChangedAtUtc,
            createdBy: changeContext.UserId);

        return new AccessRule(
            Guid.NewGuid(),
            targetType,
            normalizedTargetId,
            subjectType,
            normalizedSubjectId,
            mode,
            isActive: true,
            auditInfo);
    }

    public void UpdateMode(
        AccessRuleMode mode,
        ChangeContext changeContext)
    {
        if (Mode == mode)
        {
            return;
        }

        Mode = mode;
        Touch(changeContext);
    }

    public void Activate(ChangeContext changeContext)
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        Touch(changeContext);
    }

    public void Deactivate(ChangeContext changeContext)
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        Touch(changeContext);
    }

    public bool MatchesTarget(
        AccessTargetType targetType,
        string targetId)
    {
        return TargetType == targetType &&
               string.Equals(TargetId, NormalizeRequired(targetId, nameof(targetId)), StringComparison.OrdinalIgnoreCase);
    }

    public bool MatchesSubject(
        AccessSubjectType subjectType,
        string subjectId)
    {
        return SubjectType == subjectType &&
               string.Equals(SubjectId, NormalizeRequired(subjectId, nameof(subjectId)), StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"{parameterName} is required.");
        }

        return value.Trim();
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
