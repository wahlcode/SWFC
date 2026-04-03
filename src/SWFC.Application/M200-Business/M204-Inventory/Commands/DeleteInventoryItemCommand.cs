namespace SWFC.Application.M200_Business.M204_Inventory.Commands;

public sealed record DeleteInventoryItemCommand(
    Guid Id,
    string Reason);