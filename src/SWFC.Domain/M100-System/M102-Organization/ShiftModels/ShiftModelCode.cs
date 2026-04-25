using SWFC.Domain.M100_System.M101_Foundation.Exceptions;

namespace SWFC.Domain.M100_System.M102_Organization.ShiftModels;

public sealed class ShiftModelCode
{
    private ShiftModelCode(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static ShiftModelCode Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException("Shift model code is required.");
        }

        var normalized = value.Trim();

        if (normalized.Length > 100)
        {
            throw new ValidationException("Shift model code must not exceed 100 characters.");
        }

        return new ShiftModelCode(normalized);
    }

    public override string ToString() => Value;
}
