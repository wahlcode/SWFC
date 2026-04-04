using SWFC.Domain.Common.Exceptions;

namespace SWFC.Domain.M100_System.M102_Organization.ValueObjects;

public sealed class UserDisplayName
{
    private UserDisplayName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static UserDisplayName Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException("User display name is required.");
        }

        var normalized = value.Trim();

        if (normalized.Length > 200)
        {
            throw new ValidationException("User display name must not exceed 200 characters.");
        }

        return new UserDisplayName(normalized);
    }

    public override string ToString() => Value;
}