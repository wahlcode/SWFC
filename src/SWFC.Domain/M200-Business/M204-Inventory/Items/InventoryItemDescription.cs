using SWFC.Domain.M100_System.M101_Foundation.Errors;
using SWFC.Domain.M100_System.M101_Foundation.Exceptions;

namespace SWFC.Domain.M200_Business.M204_Inventory.Items;

public sealed class InventoryItemDescription
{
    private InventoryItemDescription(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static InventoryItemDescription Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException(ValidationErrorCodes.Invalid);
        }

        var normalized = value.Trim();

        if (normalized.Length > 500)
        {
            throw new ValidationException(ValidationErrorCodes.Invalid);
        }

        return new InventoryItemDescription(normalized);
    }

    public override string ToString() => Value;
}

