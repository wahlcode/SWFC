using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;

namespace SWFC.Domain.M200_Business.M205_Energy.EnergyMeters;

public sealed class EnergyMeter
{
    private EnergyMeter()
    {
        Id = Guid.Empty;
        Name = null!;
        MediumName = null!;
        Unit = null!;
        AuditInfo = null!;
    }

    private EnergyMeter(
        Guid id,
        EnergyMeterName name,
        EnergyMediumType mediumType,
        EnergyMediumName mediumName,
        EnergyMeterUnit unit,
        bool isManualEntryEnabled,
        bool isExternalImportEnabled,
        EnergyExternalSystem? externalSystem,
        EnergyMeterRfidTag? rfidTag,
        bool supportsOfflineCapture,
        Guid? parentMeterId,
        Guid? machineId,
        bool isActive,
        AuditInfo auditInfo)
    {
        Id = id;
        Name = name;
        MediumType = mediumType;
        MediumName = mediumName;
        Unit = unit;
        IsManualEntryEnabled = isManualEntryEnabled;
        IsExternalImportEnabled = isExternalImportEnabled;
        ExternalSystem = externalSystem;
        RfidTag = rfidTag;
        SupportsOfflineCapture = supportsOfflineCapture;
        ParentMeterId = parentMeterId;
        MachineId = machineId;
        IsActive = isActive;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public EnergyMeterName Name { get; private set; }
    public EnergyMediumType MediumType { get; private set; }
    public EnergyMediumName MediumName { get; private set; }
    public EnergyMeterUnit Unit { get; private set; }
    public bool IsManualEntryEnabled { get; private set; }
    public bool IsExternalImportEnabled { get; private set; }
    public EnergyExternalSystem? ExternalSystem { get; private set; }
    public EnergyMeterRfidTag? RfidTag { get; private set; }
    public bool SupportsOfflineCapture { get; private set; }
    public Guid? ParentMeterId { get; private set; }
    public Guid? MachineId { get; private set; }
    public bool IsActive { get; private set; }
    public AuditInfo AuditInfo { get; private set; }

    public static EnergyMeter Create(
        EnergyMeterName name,
        EnergyMediumType mediumType,
        EnergyMediumName mediumName,
        EnergyMeterUnit unit,
        bool isManualEntryEnabled,
        bool isExternalImportEnabled,
        EnergyExternalSystem? externalSystem,
        EnergyMeterRfidTag? rfidTag,
        bool supportsOfflineCapture,
        Guid? parentMeterId,
        Guid? machineId,
        ChangeContext changeContext)
    {
        var auditInfo = new AuditInfo(
            changeContext.ChangedAtUtc,
            changeContext.UserId);

        return new EnergyMeter(
            Guid.NewGuid(),
            name,
            mediumType,
            mediumName,
            unit,
            isManualEntryEnabled,
            isExternalImportEnabled,
            externalSystem,
            rfidTag,
            supportsOfflineCapture,
            parentMeterId,
            machineId,
            true,
            auditInfo);
    }

    public void Update(
        EnergyMeterName name,
        EnergyMediumType mediumType,
        EnergyMediumName mediumName,
        EnergyMeterUnit unit,
        bool isManualEntryEnabled,
        bool isExternalImportEnabled,
        EnergyExternalSystem? externalSystem,
        EnergyMeterRfidTag? rfidTag,
        bool supportsOfflineCapture,
        Guid? parentMeterId,
        Guid? machineId,
        ChangeContext changeContext)
    {
        EnsureValidHierarchy(parentMeterId);

        Name = name;
        MediumType = mediumType;
        MediumName = mediumName;
        Unit = unit;
        IsManualEntryEnabled = isManualEntryEnabled;
        IsExternalImportEnabled = isExternalImportEnabled;
        ExternalSystem = externalSystem;
        RfidTag = rfidTag;
        SupportsOfflineCapture = supportsOfflineCapture;
        ParentMeterId = parentMeterId;
        MachineId = machineId;

        Touch(changeContext);
    }

    public void SetActiveState(bool isActive, ChangeContext changeContext)
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
            AuditInfo.CreatedAtUtc,
            AuditInfo.CreatedBy,
            changeContext.ChangedAtUtc,
            changeContext.UserId);
    }

    private void EnsureValidHierarchy(Guid? parentMeterId)
    {
        if (parentMeterId == Id)
        {
            throw new ArgumentException("A meter cannot be its own parent.", nameof(parentMeterId));
        }
    }
}
