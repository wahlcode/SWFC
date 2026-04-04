using SWFC.Domain.Common.Errors;
using SWFC.Domain.Common.Exceptions;

namespace SWFC.Domain.M200_Business.M201_Assets.ValueObjects;

public sealed class MachineDescription
{
    private MachineDescription(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static MachineDescription Create(string? value)
    {
        var normalized = value?.Trim() ?? string.Empty;

        if (normalized.Length > 500)
        {
            throw new ValidationException(ErrorCodes.Validation.Invalid);
        }

        return new MachineDescription(normalized);
    }

    public override string ToString() => Value;
}