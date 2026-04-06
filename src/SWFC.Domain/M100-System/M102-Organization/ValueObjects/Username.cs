using SWFC.Domain.Common.Exceptions;

namespace SWFC.Domain.M100_System.M102_Organization.ValueObjects;

public sealed class Username
{
    private Username(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Username Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException("Username is required.");
        }

        var normalized = value.Trim();

        if (normalized.Length < 3)
        {
            throw new ValidationException("Username must be at least 3 characters long.");
        }

        if (normalized.Length > 100)
        {
            throw new ValidationException("Username must not exceed 100 characters.");
        }

        return new Username(normalized);
    }

    public override string ToString() => Value;
}