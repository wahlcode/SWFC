namespace SWFC.Application.M200_Business.M201_Assets.Commands;

public sealed record UpdateMachineCommand(
    Guid Id,
    string Name,
    string InventoryNumber,
    string? Location,
    string Status,
    string? Manufacturer,
    string? Model,
    string? SerialNumber,
    string? Description,
    string Reason);