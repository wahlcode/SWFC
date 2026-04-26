using SWFC.Domain.M100_System.M101_Foundation.Errors;
using SWFC.Domain.M100_System.M101_Foundation.Exceptions;

namespace SWFC.Domain.M200_Business.M204_Inventory.Items;

public sealed class InventoryItemStandardUnitPrice
{
    private InventoryItemStandardUnitPrice(decimal value)
    {
        Value = value;
    }

    public decimal Value { get; }

    public static InventoryItemStandardUnitPrice Create(decimal value)
    {
        if (value < 0 || value > 999_999_999m)
        {
            throw new ValidationException(ValidationErrorCodes.Invalid);
        }

        return new InventoryItemStandardUnitPrice(decimal.Round(value, 4));
    }

    public override string ToString() => Value.ToString("0.####");
}
