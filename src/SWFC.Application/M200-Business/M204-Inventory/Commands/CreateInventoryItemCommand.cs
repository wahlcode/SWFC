using SWFC.Domain.M200_Business.M204_Inventory.Enums;

namespace SWFC.Application.M200_Business.M204_Inventory.Commands;


public sealed record CreateInventoryItemCommand(
    string Name,
    string Reason);
