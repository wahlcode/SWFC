using SWFC.Domain.M100_System.M101_Foundation.Exceptions;

namespace SWFC.Domain.M800_Security.M806_AccessControl.Roles;

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
