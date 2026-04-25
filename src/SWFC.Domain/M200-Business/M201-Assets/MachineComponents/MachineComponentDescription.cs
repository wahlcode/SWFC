using SWFC.Domain.M100_System.M101_Foundation.Errors;
using SWFC.Domain.M100_System.M101_Foundation.Exceptions;

namespace SWFC.Domain.M200_Business.M201_Assets.MachineComponents;

public sealed class MachineComponentDescription
{
    private MachineComponentDescription(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static MachineComponentDescription Create(string? value)
    {
        var normalized = value?.Trim() ?? string.Empty;

        if (normalized.Length > 500)
        {
            throw new ValidationException(ValidationErrorCodes.Invalid);
        }

        return new MachineComponentDescription(normalized);
    }

    public override string ToString() => Value;
}
