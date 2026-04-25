namespace SWFC.Application.M200_Business.M204_Inventory.Reservations;

public sealed record StockReservationDetailsDto(
    Guid Id,
    Guid StockId,
    int Quantity,
    string Note,
    string Status,
    int? TargetType,
    string? TargetReference,
    DateTime CreatedAtUtc,
    string CreatedBy,
    DateTime? LastModifiedAtUtc,
    string? LastModifiedBy);

public sealed record StockReservationListItem(
    Guid Id,
    Guid StockId,
    Guid InventoryItemId,
    string InventoryItemName,
    Guid LocationId,
    string LocationName,
    string LocationCode,
    string? Bin,
    int Quantity,
    string Note,
    string Status,
    int? TargetType,
    string? TargetReference,
    DateTime CreatedAtUtc,
    string CreatedBy,
    DateTime? LastModifiedAtUtc,
    string? LastModifiedBy);

