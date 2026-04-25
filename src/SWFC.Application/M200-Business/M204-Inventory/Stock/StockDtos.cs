using SWFC.Domain.M200_Business.M204_Inventory.Stock;
using SWFC.Domain.M200_Business.M204_Inventory.Reservations;

namespace SWFC.Application.M200_Business.M204_Inventory.Stock;

public sealed record StockAvailabilityDto(
    Guid StockId,
    Guid InventoryItemId,
    string InventoryItemName,
    Guid LocationId,
    string LocationName,
    string LocationCode,
    string? Bin,
    int QuantityOnHand,
    int ReservedQuantity,
    int AvailableQuantity);

public sealed record StockLookupItem(
    Guid StockId,
    Guid InventoryItemId,
    string InventoryItemArticleNumber,
    string InventoryItemName,
    Guid LocationId,
    string LocationName,
    string LocationCode,
    string? Bin,
    int QuantityOnHand,
    int ReservedQuantity,
    int AvailableQuantity);

public sealed record StockMovementDetailsDto(
    Guid Id,
    Guid StockId,
    StockMovementType MovementType,
    int QuantityDelta,
    InventoryTargetType? TargetType,
    string? TargetReference,
    DateTime CreatedAtUtc,
    string CreatedBy);

public sealed record StockMovementListItem(
    Guid Id,
    Guid StockId,
    Guid InventoryItemId,
    string InventoryItemName,
    Guid LocationId,
    string LocationName,
    string LocationCode,
    string? Bin,
    StockMovementType MovementType,
    int QuantityDelta,
    InventoryTargetType? TargetType,
    string? TargetReference,
    DateTime CreatedAtUtc,
    string CreatedBy);
