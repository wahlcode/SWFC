using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;

namespace SWFC.Domain.M800_Security.M806_AccessControl.Assignments;

public sealed class UserRole
{
    private UserRole()
    {
        Id = Guid.Empty;
        UserId = Guid.Empty;
        RoleId = Guid.Empty;
        AuditInfo = null!;
    }

    private UserRole(
        Guid id,
        Guid userId,
        Guid roleId,
        bool isActive,
        AuditInfo auditInfo)
    {
        Id = id;
        UserId = userId;
        RoleId = roleId;
        IsActive = isActive;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }
    public bool IsActive { get; private set; }
    public AuditInfo AuditInfo { get; private set; }

    public static UserRole Create(Guid userId, Guid roleId, ChangeContext changeContext)
    {
        var auditInfo = new AuditInfo(
            createdAtUtc: changeContext.ChangedAtUtc,
            createdBy: changeContext.UserId);

        return new UserRole(
            Guid.NewGuid(),
            userId,
            roleId,
            isActive: true,
            auditInfo);
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

    private void Touch(ChangeContext changeContext)
    {
        AuditInfo = new AuditInfo(
            createdAtUtc: AuditInfo.CreatedAtUtc,
            createdBy: AuditInfo.CreatedBy,
            lastModifiedAtUtc: changeContext.ChangedAtUtc,
            lastModifiedBy: changeContext.UserId);
    }
}
