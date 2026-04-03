namespace SWFC.Application.M200_Business.M204_Inventory.Commands;

public sealed record UpdateInventoryItemCommand(
    Guid Id,
    string Name,
    string Reason);