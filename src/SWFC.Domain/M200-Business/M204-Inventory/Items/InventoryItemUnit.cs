using SWFC.Domain.M100_System.M101_Foundation.Errors;
using SWFC.Domain.M100_System.M101_Foundation.Exceptions;

namespace SWFC.Domain.M200_Business.M204_Inventory.Items;

public sealed class InventoryItemUnit
{
    private InventoryItemUnit(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static InventoryItemUnit Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException(ValidationErrorCodes.Invalid);
        }

        var normalized = value.Trim();

        if (normalized.Length > 20)
        {
            throw new ValidationException(ValidationErrorCodes.Invalid);
        }

        return new InventoryItemUnit(normalized);
    }

    public override string ToString() => Value;
}

