using SWFC.Domain.M100_System.M101_Foundation.Errors;
using SWFC.Domain.M100_System.M101_Foundation.Exceptions;

namespace SWFC.Domain.M200_Business.M201_Assets.MachineComponents;

public sealed class MachineComponentName
{
    private MachineComponentName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static MachineComponentName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException(ValidationErrorCodes.Invalid);
        }

        var normalized = value.Trim();

        if (normalized.Length > 100)
        {
            throw new ValidationException(ValidationErrorCodes.Invalid);
        }

        return new MachineComponentName(normalized);
    }

    public override string ToString() => Value;
}
