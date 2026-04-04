namespace SWFC.Application.M200_Business.M204_Inventory.Queries;

public sealed record GetStockAvailabilityQuery(Guid StockId);

public sealed record StockAvailabilityDto(
    Guid StockId,
    int QuantityOnHand,
    int ReservedQuantity,
    int AvailableQuantity);