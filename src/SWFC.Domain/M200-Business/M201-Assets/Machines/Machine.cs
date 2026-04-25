using SWFC.Domain.M100_System.M101_Foundation.Exceptions;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M200_Business.M201_Assets.Hierarchy;

namespace SWFC.Domain.M200_Business.M201_Assets.Machines;

public sealed class Machine
{
    private readonly List<Machine> _children = new();

    private Machine()
    {
        Id = Guid.Empty;
        Name = null!;
        InventoryNumber = null!;
        Location = null!;
        Status = null!;
        Manufacturer = null!;
        Model = null!;
        SerialNumber = null!;
        Description = null!;
        AuditInfo = null!;
    }

    private Machine(
        Guid id,
        MachineName name,
        MachineInventoryNumber inventoryNumber,
        MachineLocation location,
        MachineStatus status,
        MachineManufacturer manufacturer,
        MachineModel model,
        MachineSerialNumber serialNumber,
        MachineDescription description,
        Guid? parentMachineId,
        Guid? organizationUnitId,
        bool isActive,
        AuditInfo auditInfo)
    {
        Id = id;
        Name = name;
        InventoryNumber = inventoryNumber;
        Location = location;
        Status = status;
        Manufacturer = manufacturer;
        Model = model;
        SerialNumber = serialNumber;
        Description = description;
        ParentMachineId = parentMachineId;
        OrganizationUnitId = organizationUnitId;
        IsActive = isActive;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public MachineName Name { get; private set; }
    public MachineInventoryNumber InventoryNumber { get; private set; }
    public MachineLocation Location { get; private set; }
    public MachineStatus Status { get; private set; }
    public MachineManufacturer Manufacturer { get; private set; }
    public MachineModel Model { get; private set; }
    public MachineSerialNumber SerialNumber { get; private set; }
    public MachineDescription Description { get; private set; }

    public Guid? ParentMachineId { get; private set; }
    public Machine? ParentMachine { get; private set; }

    public Guid? OrganizationUnitId { get; private set; }
    public bool IsActive { get; private set; }

    public IReadOnlyCollection<Machine> Children => _children.AsReadOnly();

    public AuditInfo AuditInfo { get; private set; }

    public static Machine Create(
        MachineName name,
        MachineInventoryNumber inventoryNumber,
        MachineLocation location,
        MachineStatus status,
        MachineManufacturer manufacturer,
        MachineModel model,
        MachineSerialNumber serialNumber,
        MachineDescription description,
        Guid? parentMachineId,
        Guid? organizationUnitId,
        ChangeContext changeContext)
    {
        var auditInfo = new AuditInfo(
            createdAtUtc: changeContext.ChangedAtUtc,
            createdBy: changeContext.UserId);

        return new Machine(
            Guid.NewGuid(),
            name,
            inventoryNumber,
            location,
            status,
            manufacturer,
            model,
            serialNumber,
            description,
            parentMachineId,
            organizationUnitId,
            isActive: true,
            auditInfo);
    }

    public void UpdateDetails(
        MachineName name,
        MachineInventoryNumber inventoryNumber,
        MachineLocation location,
        MachineStatus status,
        MachineManufacturer manufacturer,
        MachineModel model,
        MachineSerialNumber serialNumber,
        MachineDescription description,
        ChangeContext changeContext)
    {
        Name = name;
        InventoryNumber = inventoryNumber;
        Location = location;
        Status = status;
        Manufacturer = manufacturer;
        Model = model;
        SerialNumber = serialNumber;
        Description = description;

        Touch(changeContext);
    }

    public void AssignParent(
        Machine? parentMachine,
        IReadOnlyCollection<Guid> descendantIds,
        ChangeContext changeContext)
    {
        if (parentMachine is null)
        {
            ParentMachineId = null;
            ParentMachine = null;

            Touch(changeContext);
            return;
        }

        if (!MachineHierarchyRules.CanAssignParent(Id, parentMachine.Id, descendantIds))
        {
            throw new DomainException(
                "The selected parent machine would create an invalid machine hierarchy.");
        }

        ParentMachineId = parentMachine.Id;
        ParentMachine = parentMachine;

        Touch(changeContext);
    }

    public void AssignOrganizationUnit(Guid? organizationUnitId, ChangeContext changeContext)
    {
        OrganizationUnitId = organizationUnitId;
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

    public bool HasChildren()
    {
        return _children.Count > 0;
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
