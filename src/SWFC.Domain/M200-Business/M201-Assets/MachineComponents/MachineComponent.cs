using SWFC.Domain.M100_System.M101_Foundation.Exceptions;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M200_Business.M201_Assets.MachineComponentAreas;

namespace SWFC.Domain.M200_Business.M201_Assets.MachineComponents;

public sealed class MachineComponent
{
    private MachineComponent()
    {
        Id = Guid.Empty;
        Name = null!;
        Description = null!;
        AuditInfo = null!;
    }

    private MachineComponent(
        Guid id,
        Guid machineId,
        Guid? machineComponentAreaId,
        Guid? parentMachineComponentId,
        MachineComponentName name,
        MachineComponentDescription description,
        bool isActive,
        AuditInfo auditInfo)
    {
        Id = id;
        MachineId = machineId;
        MachineComponentAreaId = machineComponentAreaId;
        ParentMachineComponentId = parentMachineComponentId;
        Name = name;
        Description = description;
        IsActive = isActive;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public Guid MachineId { get; private set; }
    public Guid? MachineComponentAreaId { get; private set; }
    public Guid? ParentMachineComponentId { get; private set; }
    public MachineComponentName Name { get; private set; }
    public MachineComponentDescription Description { get; private set; }
    public bool IsActive { get; private set; }
    public AuditInfo AuditInfo { get; private set; }

    public static MachineComponent Create(
        Guid machineId,
        Guid? machineComponentAreaId,
        MachineComponent? parentComponent,
        MachineComponentName name,
        MachineComponentDescription description,
        ChangeContext changeContext)
    {
        if (parentComponent is not null &&
            !MachineComponentHierarchyRules.CanAssignParentForNewComponent(machineId, parentComponent.MachineId))
        {
            throw new DomainException(
                "Parent and child components must belong to the same machine.");
        }

        var auditInfo = new AuditInfo(
            createdAtUtc: changeContext.ChangedAtUtc,
            createdBy: changeContext.UserId);

        return new MachineComponent(
            Guid.NewGuid(),
            machineId,
            machineComponentAreaId,
            parentComponent?.Id,
            name,
            description,
            true,
            auditInfo);
    }

    public void Update(
        Guid? machineComponentAreaId,
        MachineComponentName name,
        MachineComponentDescription description,
        ChangeContext changeContext)
    {
        Name = name;
        Description = description;
        MachineComponentAreaId = machineComponentAreaId;

        Touch(changeContext);
    }

    public void Move(
        Guid targetMachineId,
        Guid? targetMachineComponentAreaId,
        MachineComponent? parentComponent,
        IReadOnlyCollection<Guid> descendantIds,
        ChangeContext changeContext)
    {
        if (parentComponent is null)
        {
            MachineId = targetMachineId;
            MachineComponentAreaId = targetMachineComponentAreaId;
            ParentMachineComponentId = null;

            Touch(changeContext);
            return;
        }

        if (!MachineComponentHierarchyRules.CanAssignParent(
                Id,
                targetMachineId,
                parentComponent.Id,
                parentComponent.MachineId,
                descendantIds))
        {
            throw new DomainException(
                "The selected parent component would create an invalid component hierarchy.");
        }

        MachineId = targetMachineId;
        MachineComponentAreaId = targetMachineComponentAreaId;
        ParentMachineComponentId = parentComponent.Id;

        Touch(changeContext);
    }

    public void SetActiveState(
        bool isActive,
        bool hasChildren,
        ChangeContext changeContext)
    {
        if (IsActive == isActive)
        {
            return;
        }

        if (!isActive && hasChildren)
        {
            throw new DomainException(
                "Components with child components cannot be deactivated. Deactivate child components first.");
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
