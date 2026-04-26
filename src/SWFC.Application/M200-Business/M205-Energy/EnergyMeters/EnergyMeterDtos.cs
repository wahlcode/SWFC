using SWFC.Domain.M200_Business.M205_Energy.EnergyMeters;

namespace SWFC.Application.M200_Business.M205_Energy.EnergyMeters;

public sealed record EnergyMeterListItemDto(
    Guid Id,
    string Name,
    EnergyMediumType MediumType,
    string MediumName,
    string Unit,
    bool IsManualEntryEnabled,
    bool IsExternalImportEnabled,
    string? ExternalSystem,
    string? RfidTag,
    bool SupportsOfflineCapture,
    Guid? ParentMeterId,
    Guid? MachineId,
    bool IsActive,
    DateTime CreatedAtUtc,
    string CreatedBy,
    DateTime? LastModifiedAtUtc,
    string? LastModifiedBy);

public sealed record EnergyMeterDetailsDto(
    Guid Id,
    string Name,
    EnergyMediumType MediumType,
    string MediumName,
    string Unit,
    bool IsManualEntryEnabled,
    bool IsExternalImportEnabled,
    string? ExternalSystem,
    string? RfidTag,
    bool SupportsOfflineCapture,
    Guid? ParentMeterId,
    Guid? MachineId,
    bool IsActive,
    DateTime CreatedAtUtc,
    string CreatedBy,
    DateTime? LastModifiedAtUtc,
    string? LastModifiedBy);
