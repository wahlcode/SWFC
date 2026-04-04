namespace SWFC.Application.M200_Business.M204_Inventory.Queries;

public sealed record GetStockReservationsQuery;

public sealed record StockReservationListItem(
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