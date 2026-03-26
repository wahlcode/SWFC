using SWFC.Domain.Common.Errors;
using SWFC.Domain.Common.Exceptions;

namespace SWFC.Domain.M200_Business.M201_Assets.ValueObjects;

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
            throw new ValidationException(ErrorCodes.Machine.NameRequired);
        }

        var normalized = value.Trim();

        if (normalized.Length > 100)
        {
            throw new ValidationException(ErrorCodes.Machine.NameTooLong);
        }

        return new MachineName(normalized);
    }

    public override string ToString() => Value;
}