using SWFC.Domain.Common.ValueObjects;
using SWFC.Domain.M200_Business.M201_Assets.ValueObjects;

namespace SWFC.Domain.M200_Business.M201_Assets.Entities;

public sealed class Machine
{
    private Machine()
    {
        Id = Guid.Empty;
        Name = null!;
        AuditInfo = null!;
    }

    private Machine(Guid id, MachineName name, AuditInfo auditInfo)
    {
        Id = id;
        Name = name;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public MachineName Name { get; private set; }
    public AuditInfo AuditInfo { get; private set; }

    public static Machine Create(MachineName name, ChangeContext changeContext)
    {
        var auditInfo = new AuditInfo(
            createdAtUtc: changeContext.ChangedAtUtc,
            createdBy: changeContext.UserId);

        return new Machine(Guid.NewGuid(), name, auditInfo);
    }

    public void Rename(MachineName name, ChangeContext changeContext)
    {
        Name = name;
        AuditInfo = new AuditInfo(
            createdAtUtc: AuditInfo.CreatedAtUtc,
            createdBy: AuditInfo.CreatedBy,
            lastModifiedAtUtc: changeContext.ChangedAtUtc,
            lastModifiedBy: changeContext.UserId);
    }
}