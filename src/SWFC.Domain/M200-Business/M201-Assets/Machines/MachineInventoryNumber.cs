using SWFC.Domain.M100_System.M101_Foundation.Errors;
using SWFC.Domain.M100_System.M101_Foundation.Exceptions;

namespace SWFC.Domain.M200_Business.M201_Assets.Machines;

public sealed class MachineInventoryNumber
{
    private MachineInventoryNumber(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static MachineInventoryNumber Create(string value)
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

        return new MachineInventoryNumber(normalized);
    }

    public override string ToString() => Value;
}

