using SWFC.Domain.M200_Business.M204_Inventory.Entities;

namespace SWFC.Application.M200_Business.M204_Inventory.Interfaces;

public interface IInventoryAvailabilityCalculator
{
    StockAvailabilityResult Calculate(Stock stock);
}

public sealed record StockAvailabilityResult(
    int QuantityOnHand,
    int ReservedQuantity,
    int AvailableQuantity);