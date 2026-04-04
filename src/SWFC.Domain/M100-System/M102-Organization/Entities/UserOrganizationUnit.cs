using SWFC.Domain.Common.ValueObjects;

namespace SWFC.Domain.M100_System.M102_Organization.Entities;

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
        AuditInfo auditInfo)
    {
        Id = id;
        UserId = userId;
        OrganizationUnitId = organizationUnitId;
        IsPrimary = isPrimary;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid OrganizationUnitId { get; private set; }
    public bool IsPrimary { get; private set; }
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
            auditInfo);
    }

    public void SetPrimary(
        bool isPrimary,
        ChangeContext changeContext)
    {
        IsPrimary = isPrimary;

        AuditInfo = new AuditInfo(
            createdAtUtc: AuditInfo.CreatedAtUtc,
            createdBy: AuditInfo.CreatedBy,
            lastModifiedAtUtc: changeContext.ChangedAtUtc,
            lastModifiedBy: changeContext.UserId);
    }
}