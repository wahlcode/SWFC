namespace SWFC.Application.M200_Business.M201_Assets.Machines;

public sealed record MachineChildListItemDto(
    Guid Id,
    string Name,
    string InventoryNumber,
    string Status);

public sealed record MachineDetailsDto(
    Guid Id,
    string Name,
    string InventoryNumber,
    string Location,
    string Status,
    string Manufacturer,
    string Model,
    string SerialNumber,
    string Description,
    Guid? ParentMachineId,
    string? ParentMachineName,
    Guid? OrganizationUnitId,
    string? OrganizationUnitName,
    IReadOnlyList<MachineChildListItemDto> Children,
    DateTime CreatedAtUtc,
    string CreatedBy,
    DateTime? LastModifiedAtUtc,
    string? LastModifiedBy);

public sealed record MachineSelectionOptionDto(
    Guid Id,
    string Name,
    string InventoryNumber,
    int Level);

public sealed record MachineListItem(
    Guid Id,
    string Name,
    string InventoryNumber,
    string Location,
    string Status,
    string Manufacturer,
    string Model,
    string SerialNumber,
    Guid? ParentMachineId,
    string? ParentMachineName,
    int Level,
    bool HasChildren,
    DateTime CreatedAtUtc,
    string CreatedBy,
    DateTime? LastModifiedAtUtc,
    string? LastModifiedBy);

public sealed record OrganizationUnitSelectionOptionDto(
    Guid Id,
    string Name);

