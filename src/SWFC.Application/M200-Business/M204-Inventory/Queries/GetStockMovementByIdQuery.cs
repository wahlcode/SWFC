using SWFC.Domain.M200_Business.M204_Inventory.Enums;

namespace SWFC.Application.M200_Business.M204_Inventory.Queries;

public sealed record GetStockMovementByIdQuery(Guid Id);

public sealed record StockMovementDetailsDto(
    Guid Id,
    Guid StockId,
    StockMovementType MovementType,
    int QuantityDelta,
    DateTime CreatedAtUtc,
    string CreatedBy);