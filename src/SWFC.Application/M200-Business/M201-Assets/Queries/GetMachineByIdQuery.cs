namespace SWFC.Application.M200_Business.M201_Assets.Queries;

public sealed record GetMachineByIdQuery(Guid Id);

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
    DateTime CreatedAtUtc,
    string CreatedBy,
    DateTime? LastModifiedAtUtc,
    string? LastModifiedBy);