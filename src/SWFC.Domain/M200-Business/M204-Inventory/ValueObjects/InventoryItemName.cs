using SWFC.Domain.Common.Errors;
using SWFC.Domain.Common.Exceptions;

namespace SWFC.Domain.M200_Business.M204_Inventory.ValueObjects;

public sealed class InventoryItemName
{
    private InventoryItemName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static InventoryItemName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException(ErrorCodes.Validation.Invalid);
        }

        var normalized = value.Trim();

        if (normalized.Length > 100)
        {
            throw new ValidationException(ErrorCodes.Validation.Invalid);
        }

        return new InventoryItemName(normalized);
    }

    public override string ToString() => Value;
}