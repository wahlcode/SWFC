using SWFC.Domain.M100_System.M101_Foundation.Errors;
using SWFC.Domain.M100_System.M101_Foundation.Exceptions;

namespace SWFC.Domain.M200_Business.M204_Inventory.Items;

public sealed class InventoryItemCurrency
{
    private InventoryItemCurrency(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static InventoryItemCurrency Create(string? value)
    {
        var normalized = string.IsNullOrWhiteSpace(value)
            ? "EUR"
            : value.Trim().ToUpperInvariant();

        if (normalized.Length != 3 || normalized.Any(x => x < 'A' || x > 'Z'))
        {
            throw new ValidationException(ValidationErrorCodes.Invalid);
        }

        return new InventoryItemCurrency(normalized);
    }

    public override string ToString() => Value;
}
