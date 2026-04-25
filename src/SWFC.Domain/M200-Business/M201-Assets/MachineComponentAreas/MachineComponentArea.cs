using SWFC.Domain.M100_System.M101_Foundation.Exceptions;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;

namespace SWFC.Domain.M200_Business.M201_Assets.MachineComponentAreas;

public sealed class MachineComponentArea
{
    private MachineComponentArea()
    {
        Id = Guid.Empty;
        Name = null!;
        AuditInfo = null!;
    }

    private MachineComponentArea(
        Guid id,
        MachineComponentAreaName name,
        bool isActive,
        AuditInfo auditInfo)
    {
        Id = id;
        Name = name;
        IsActive = isActive;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public MachineComponentAreaName Name { get; private set; }
    public bool IsActive { get; private set; }
    public AuditInfo AuditInfo { get; private set; }

    public static MachineComponentArea Create(
        MachineComponentAreaName name,
        ChangeContext changeContext)
    {
        var auditInfo = new AuditInfo(
            createdAtUtc: changeContext.ChangedAtUtc,
            createdBy: changeContext.UserId);

        return new MachineComponentArea(
            Guid.NewGuid(),
            name,
            true,
            auditInfo);
    }

    public void Update(
        MachineComponentAreaName name,
        ChangeContext changeContext)
    {
        if (!IsActive)
        {
            throw new DomainException("Inactive component areas cannot be updated.");
        }

        Name = name;
        Touch(changeContext);
    }

    public void SetActiveState(
        bool isActive,
        ChangeContext changeContext)
    {
        if (IsActive == isActive)
        {
            return;
        }

        IsActive = isActive;
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
