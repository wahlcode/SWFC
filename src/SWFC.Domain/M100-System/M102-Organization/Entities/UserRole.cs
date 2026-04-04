using SWFC.Domain.Common.ValueObjects;

namespace SWFC.Domain.M100_System.M102_Organization.Entities;

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
        AuditInfo auditInfo)
    {
        Id = id;
        UserId = userId;
        RoleId = roleId;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }
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
            auditInfo);
    }
}