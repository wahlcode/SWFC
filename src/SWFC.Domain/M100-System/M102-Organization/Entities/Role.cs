using SWFC.Domain.Common.ValueObjects;
using SWFC.Domain.M100_System.M102_Organization.ValueObjects;

namespace SWFC.Domain.M100_System.M102_Organization.Entities;

public sealed class Role
{
    private Role()
    {
        Id = Guid.Empty;
        Name = null!;
        Description = null;
        AuditInfo = null!;
    }

    private Role(
        Guid id,
        RoleName name,
        string? description,
        AuditInfo auditInfo)
    {
        Id = id;
        Name = name;
        Description = description;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public RoleName Name { get; private set; }
    public string? Description { get; private set; }
    public AuditInfo AuditInfo { get; private set; }

    public static Role Create(
        RoleName name,
        string? description,
        ChangeContext changeContext)
    {
        var normalizedDescription = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();

        var auditInfo = new AuditInfo(
            createdAtUtc: changeContext.ChangedAtUtc,
            createdBy: changeContext.UserId);

        return new Role(
            Guid.NewGuid(),
            name,
            normalizedDescription,
            auditInfo);
    }

    public void UpdateDetails(
        RoleName name,
        string? description,
        ChangeContext changeContext)
    {
        Name = name;
        Description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();

        AuditInfo = new AuditInfo(
            createdAtUtc: AuditInfo.CreatedAtUtc,
            createdBy: AuditInfo.CreatedBy,
            lastModifiedAtUtc: changeContext.ChangedAtUtc,
            lastModifiedBy: changeContext.UserId);
    }
}