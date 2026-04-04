using SWFC.Domain.Common.Errors;
using SWFC.Domain.Common.Exceptions;

namespace SWFC.Domain.M200_Business.M201_Assets.ValueObjects;

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
            throw new ValidationException(ErrorCodes.Validation.Invalid);
        }

        var normalized = value.Trim();

        if (normalized.Length > 50)
        {
            throw new ValidationException(ErrorCodes.Validation.Invalid);
        }

        return new MachineInventoryNumber(normalized);
    }

    public override string ToString() => Value;
}