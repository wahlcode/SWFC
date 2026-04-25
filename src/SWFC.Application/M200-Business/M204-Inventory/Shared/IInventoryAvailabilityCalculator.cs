using StockEntity = SWFC.Domain.M200_Business.M204_Inventory.Stock.Stock;

namespace SWFC.Application.M200_Business.M204_Inventory.Shared;

public interface IInventoryAvailabilityCalculator
{
    StockAvailabilityResult Calculate(StockEntity stock);
}

public sealed record StockAvailabilityResult(
    int QuantityOnHand,
    int ReservedQuantity,
    int AvailableQuantity);

