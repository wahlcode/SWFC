using SWFC.Domain.M100_System.M101_Foundation.Errors;
using SWFC.Domain.M100_System.M101_Foundation.Exceptions;

namespace SWFC.Domain.M200_Business.M204_Inventory.Locations;

public sealed class LocationCode
{
    private LocationCode(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static LocationCode Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException(ValidationErrorCodes.Invalid);
        }

        var normalized = value.Trim();

        if (normalized.Length > 50)
        {
            throw new ValidationException(ValidationErrorCodes.Invalid);
        }

        return new LocationCode(normalized);
    }

    public override string ToString() => Value;
}

