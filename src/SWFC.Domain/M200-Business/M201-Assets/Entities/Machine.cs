using SWFC.Domain.Common.ValueObjects;
using SWFC.Domain.M200_Business.M201_Assets.ValueObjects;

namespace SWFC.Domain.M200_Business.M201_Assets.Entities;

public sealed class Machine
{
    private Machine(Guid id, MachineName name, AuditInfo auditInfo)
    {
        Id = id;
        Name = name;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; }
    public MachineName Name { get; }
    public AuditInfo AuditInfo { get; }

    public static Machine Create(MachineName name, ChangeContext changeContext)
    {
        var auditInfo = new AuditInfo(
            createdAtUtc: changeContext.ChangedAtUtc,
            createdBy: changeContext.UserId);

        return new Machine(Guid.NewGuid(), name, auditInfo);
    }
}