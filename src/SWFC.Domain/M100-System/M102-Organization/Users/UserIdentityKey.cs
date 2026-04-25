using SWFC.Domain.M100_System.M101_Foundation.Exceptions;

namespace SWFC.Domain.M100_System.M102_Organization.Users;

public sealed class UserIdentityKey
{
    private UserIdentityKey(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static UserIdentityKey Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException("User identity key is required.");
        }

        var normalized = value.Trim();

        if (normalized.Length > 200)
        {
            throw new ValidationException("User identity key must not exceed 200 characters.");
        }

        return new UserIdentityKey(normalized);
    }

    public override string ToString() => Value;
}

