namespace SWFC.Application.M200_Business.M204_Inventory.Queries;

public sealed record GetInventoryItemsQuery;

public sealed record InventoryItemListItem(
    Guid Id,
    string Name,
    Guid? StockId,
    int QuantityOnHand,
    DateTime CreatedAtUtc,
    string CreatedBy,
    DateTime? LastModifiedAtUtc,
    string? LastModifiedBy);