using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;

namespace SWFC.Domain.M100_System.M102_Organization.OrganizationUnits;

public sealed class OrganizationUnit
{
    private OrganizationUnit()
    {
        Id = Guid.Empty;
        Name = null!;
        Code = null!;
        ParentOrganizationUnitId = null;
        IsActive = true;
        AuditInfo = null!;
    }

    private OrganizationUnit(
        Guid id,
        OrganizationUnitName name,
        OrganizationUnitCode code,
        Guid? parentOrganizationUnitId,
        bool isActive,
        AuditInfo auditInfo)
    {
        Id = id;
        Name = name;
        Code = code;
        ParentOrganizationUnitId = parentOrganizationUnitId;
        IsActive = isActive;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public OrganizationUnitName Name { get; private set; }
    public OrganizationUnitCode Code { get; private set; }
    public Guid? ParentOrganizationUnitId { get; private set; }
    public bool IsActive { get; private set; }
    public AuditInfo AuditInfo { get; private set; }

    public static OrganizationUnit Create(
        OrganizationUnitName name,
        OrganizationUnitCode code,
        Guid? parentOrganizationUnitId,
        ChangeContext changeContext)
    {
        var auditInfo = new AuditInfo(
            createdAtUtc: changeContext.ChangedAtUtc,
            createdBy: changeContext.UserId);

        return new OrganizationUnit(
            Guid.NewGuid(),
            name,
            code,
            parentOrganizationUnitId,
            isActive: true,
            auditInfo);
    }

    public void UpdateDetails(
        OrganizationUnitName name,
        OrganizationUnitCode code,
        Guid? parentOrganizationUnitId,
        ChangeContext changeContext)
    {
        Name = name;
        Code = code;
        ParentOrganizationUnitId = parentOrganizationUnitId;

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

    private void Touch(ChangeContext changeContext)
    {
        AuditInfo = new AuditInfo(
            createdAtUtc: AuditInfo.CreatedAtUtc,
            createdBy: AuditInfo.CreatedBy,
            lastModifiedAtUtc: changeContext.ChangedAtUtc,
            lastModifiedBy: changeContext.UserId);
    }
}
