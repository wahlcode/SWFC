using SWFC.Domain.M100_System.M101_Foundation.Errors;
using SWFC.Domain.M100_System.M101_Foundation.Exceptions;

namespace SWFC.Domain.M200_Business.M201_Assets.Machines;

public sealed class MachineSerialNumber
{
    private MachineSerialNumber(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static MachineSerialNumber Create(string? value)
    {
        var normalized = value?.Trim() ?? string.Empty;

        if (normalized.Length > 100)
        {
            throw new ValidationException(ValidationErrorCodes.Invalid);
        }

        return new MachineSerialNumber(normalized);
    }

    public override string ToString() => Value;
}

