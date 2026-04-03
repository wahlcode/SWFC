using SWFC.Domain.M200_Business.M204_Inventory.Enums;

namespace SWFC.Application.M200_Business.M204_Inventory.Queries;

public sealed record GetStockMovementsQuery(Guid? StockId = null);

public sealed record StockMovementListItem(
    Guid Id,
    Guid StockId,
    StockMovementType MovementType,
    int QuantityDelta,
    DateTime CreatedAtUtc,
    string CreatedBy);