using SWFC.Domain.Common.Errors;
using SWFC.Domain.Common.Exceptions;

namespace SWFC.Domain.M200_Business.M201_Assets.ValueObjects;

public sealed class MachineManufacturer
{
    private MachineManufacturer(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static MachineManufacturer Create(string? value)
    {
        var normalized = value?.Trim() ?? string.Empty;

        if (normalized.Length > 100)
        {
            throw new ValidationException(ErrorCodes.Validation.Invalid);
        }

        return new MachineManufacturer(normalized);
    }

    public override string ToString() => Value;
}