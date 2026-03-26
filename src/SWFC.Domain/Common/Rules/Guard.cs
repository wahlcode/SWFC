using SWFC.Domain.Common.Exceptions;

namespace SWFC.Domain.Common.Rules;

public static class Guard
{
    public static void AgainstNull(object? value, string name)
    {
        if (value is null)
            throw new ValidationException($"{name} must not be null.");
    }

    public static void AgainstNullOrWhiteSpace(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ValidationException($"{name} must not be empty.");
    }

    public static void AgainstMaxLength(string? value, int maxLength, string name)
    {
        if (!string.IsNullOrWhiteSpace(value) && value.Length > maxLength)
            throw new ValidationException($"{name} must not exceed {maxLength} characters.");
    }
}