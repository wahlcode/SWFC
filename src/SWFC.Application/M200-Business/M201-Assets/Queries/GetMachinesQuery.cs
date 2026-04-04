namespace SWFC.Application.M200_Business.M201_Assets.Queries;

public sealed record GetMachinesQuery;

public sealed record MachineListItem(
    Guid Id,
    string Name,
    string InventoryNumber,
    string Location,
    string Status,
    string Manufacturer,
    string Model,
    string SerialNumber,
    DateTime CreatedAtUtc,
    string CreatedBy,
    DateTime? LastModifiedAtUtc,
    string? LastModifiedBy);