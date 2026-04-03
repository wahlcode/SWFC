namespace SWFC.Application.M200_Business.M204_Inventory.Queries;

public sealed record GetInventoryItemByIdQuery(Guid Id);

public sealed record InventoryItemDetailsDto(
    Guid Id,
    string Name,
    Guid? StockId,
    int QuantityOnHand,
    int ReservedQuantity,
    int AvailableQuantity,
    DateTime CreatedAtUtc,
    string CreatedBy,
    DateTime? LastModifiedAtUtc,
    string? LastModifiedBy);