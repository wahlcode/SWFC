using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;

namespace SWFC.Domain.M800_Security.M806_AccessControl.RolePermissions;

public sealed class RolePermission
{
    private RolePermission()
    {
        Id = Guid.Empty;
        RoleId = Guid.Empty;
        PermissionId = Guid.Empty;
        IsActive = true;
        AuditInfo = null!;
    }

    private RolePermission(
        Guid id,
        Guid roleId,
        Guid permissionId,
        bool isActive,
        AuditInfo auditInfo)
    {
        Id = id;
        RoleId = roleId;
        PermissionId = permissionId;
        IsActive = isActive;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public Guid RoleId { get; private set; }
    public Guid PermissionId { get; private set; }
    public bool IsActive { get; private set; }
    public AuditInfo AuditInfo { get; private set; }

    public static RolePermission Create(
        Guid roleId,
        Guid permissionId,
        ChangeContext changeContext)
    {
        var auditInfo = new AuditInfo(
            createdAtUtc: changeContext.ChangedAtUtc,
            createdBy: changeContext.UserId);

        return new RolePermission(
            Guid.NewGuid(),
            roleId,
            permissionId,
            true,
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
