using SWFC.Application.M200_Business.M204_Inventory.Interfaces;
using SWFC.Domain.M200_Business.M204_Inventory.Entities;

namespace SWFC.Application.M200_Business.M204_Inventory.Services;

public sealed class InventoryAvailabilityCalculator : IInventoryAvailabilityCalculator
{
    public StockAvailabilityResult Calculate(Stock stock)
    {
        var quantityOnHand = stock.QuantityOnHand;
        var reservedQuantity = stock.GetReservedQuantity();
        var availableQuantity = stock.GetAvailableQuantity();

        return new StockAvailabilityResult(
            quantityOnHand,
            reservedQuantity,
            availableQuantity);
    }
}