using SWFC.Domain.M200_Business.M201_Assets.Errors;
using SWFC.Domain.M100_System.M101_Foundation.Errors;
using SWFC.Domain.M100_System.M101_Foundation.Exceptions;

namespace SWFC.Domain.M200_Business.M201_Assets.Machines;

public sealed class MachineName
{
    private MachineName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static MachineName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException(MachineErrorCodes.NameRequired);
        }

        var normalized = value.Trim();

        if (normalized.Length > 100)
        {
            throw new ValidationException(MachineErrorCodes.NameTooLong);
        }

        return new MachineName(normalized);
    }

    public override string ToString() => Value;
}


