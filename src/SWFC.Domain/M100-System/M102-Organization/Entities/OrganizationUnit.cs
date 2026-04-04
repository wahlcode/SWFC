using SWFC.Domain.Common.ValueObjects;
using SWFC.Domain.M100_System.M102_Organization.ValueObjects;

namespace SWFC.Domain.M100_System.M102_Organization.Entities;

public sealed class OrganizationUnit
{
    private OrganizationUnit()
    {
        Id = Guid.Empty;
        Name = null!;
        Code = null!;
        ParentOrganizationUnitId = null;
        AuditInfo = null!;
    }

    private OrganizationUnit(
        Guid id,
        OrganizationUnitName name,
        OrganizationUnitCode code,
        Guid? parentOrganizationUnitId,
        AuditInfo auditInfo)
    {
        Id = id;
        Name = name;
        Code = code;
        ParentOrganizationUnitId = parentOrganizationUnitId;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public OrganizationUnitName Name { get; private set; }
    public OrganizationUnitCode Code { get; private set; }
    public Guid? ParentOrganizationUnitId { get; private set; }
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

        AuditInfo = new AuditInfo(
            createdAtUtc: AuditInfo.CreatedAtUtc,
            createdBy: AuditInfo.CreatedBy,
            lastModifiedAtUtc: changeContext.ChangedAtUtc,
            lastModifiedBy: changeContext.UserId);
    }
}