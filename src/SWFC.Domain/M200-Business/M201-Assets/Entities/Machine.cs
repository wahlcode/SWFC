using SWFC.Domain.Common.ValueObjects;
using SWFC.Domain.M200_Business.M201_Assets.ValueObjects;

namespace SWFC.Domain.M200_Business.M201_Assets.Entities;

public sealed class Machine
{
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

        AuditInfo = new AuditInfo(
            createdAtUtc: AuditInfo.CreatedAtUtc,
            createdBy: AuditInfo.CreatedBy,
            lastModifiedAtUtc: changeContext.ChangedAtUtc,
            lastModifiedBy: changeContext.UserId);
    }
}