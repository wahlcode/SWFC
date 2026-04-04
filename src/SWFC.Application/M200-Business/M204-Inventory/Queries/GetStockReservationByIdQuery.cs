namespace SWFC.Application.M200_Business.M204_Inventory.Queries;

public sealed record GetStockReservationByIdQuery(Guid Id);

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