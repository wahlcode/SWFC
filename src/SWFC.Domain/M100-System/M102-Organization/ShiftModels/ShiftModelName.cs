using SWFC.Domain.M100_System.M101_Foundation.Exceptions;

namespace SWFC.Domain.M100_System.M102_Organization.ShiftModels;

public sealed class ShiftModelName
{
    private ShiftModelName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static ShiftModelName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException("Shift model name is required.");
        }

        var normalized = value.Trim();

        if (normalized.Length > 200)
        {
            throw new ValidationException("Shift model name must not exceed 200 characters.");
        }

        return new ShiftModelName(normalized);
    }

    public override string ToString() => Value;
}
