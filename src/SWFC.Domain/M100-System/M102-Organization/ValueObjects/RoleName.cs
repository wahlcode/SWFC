using SWFC.Domain.Common.Exceptions;

namespace SWFC.Domain.M100_System.M102_Organization.ValueObjects;

public sealed class RoleName
{
    private RoleName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static RoleName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException("Role name is required.");
        }

        var normalized = value.Trim();

        if (normalized.Length > 100)
        {
            throw new ValidationException("Role name must not exceed 100 characters.");
        }

        return new RoleName(normalized);
    }

    public override string ToString() => Value;
}