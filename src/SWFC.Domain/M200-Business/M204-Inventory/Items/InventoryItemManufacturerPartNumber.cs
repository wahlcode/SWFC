using SWFC.Domain.M100_System.M101_Foundation.Errors;
using SWFC.Domain.M100_System.M101_Foundation.Exceptions;

namespace SWFC.Domain.M200_Business.M204_Inventory.Items;

public sealed class InventoryItemManufacturerPartNumber
{
    private InventoryItemManufacturerPartNumber(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static InventoryItemManufacturerPartNumber? CreateOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();

        if (normalized.Length > 100)
        {
            throw new ValidationException(ValidationErrorCodes.Invalid);
        }

        return new InventoryItemManufacturerPartNumber(normalized);
    }

    public override string ToString() => Value;
}

