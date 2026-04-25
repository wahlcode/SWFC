using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;

namespace SWFC.Domain.M100_System.M102_Organization.Assignments;

public sealed class UserOrganizationUnit
{
    private UserOrganizationUnit()
    {
        Id = Guid.Empty;
        UserId = Guid.Empty;
        OrganizationUnitId = Guid.Empty;
        IsPrimary = false;
        AuditInfo = null!;
    }

    private UserOrganizationUnit(
        Guid id,
        Guid userId,
        Guid organizationUnitId,
        bool isPrimary,
        bool isActive,
        AuditInfo auditInfo)
    {
        Id = id;
        UserId = userId;
        OrganizationUnitId = organizationUnitId;
        IsPrimary = isPrimary;
        IsActive = isActive;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid OrganizationUnitId { get; private set; }
    public bool IsPrimary { get; private set; }
    public bool IsActive { get; private set; }
    public AuditInfo AuditInfo { get; private set; }

    public static UserOrganizationUnit Create(
        Guid userId,
        Guid organizationUnitId,
        bool isPrimary,
        ChangeContext changeContext)
    {
        var auditInfo = new AuditInfo(
            createdAtUtc: changeContext.ChangedAtUtc,
            createdBy: changeContext.UserId);

        return new UserOrganizationUnit(
            Guid.NewGuid(),
            userId,
            organizationUnitId,
            isPrimary,
            isActive: true,
            auditInfo);
    }

    public void Activate(bool isPrimary, ChangeContext changeContext)
    {
        IsPrimary = isPrimary;
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
        IsPrimary = false;
        Touch(changeContext);
    }

    public void SetPrimary(
        bool isPrimary,
        ChangeContext changeContext)
    {
        IsPrimary = isPrimary;

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
