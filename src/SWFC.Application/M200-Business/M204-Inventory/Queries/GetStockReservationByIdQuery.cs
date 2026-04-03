using SWFC.Domain.M200_Business.M204_Inventory.Enums;

namespace SWFC.Application.M200_Business.M204_Inventory.Queries;

public sealed record GetStockReservationByIdQuery(Guid Id);

public sealed record StockReservationDetailsDto(
    Guid Id,
    Guid StockId,
    int Quantity,
    string Note,
    StockReservationStatus Status,
    DateTime CreatedAtUtc,
    string CreatedBy,
    DateTime? LastModifiedAtUtc,
    string? LastModifiedBy);