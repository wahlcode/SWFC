using SWFC.Domain.M100_System.M101_Foundation.Rules;

namespace SWFC.Domain.M200_Business.M205_Energy.EnergyReadings;

public enum EnergyReadingSource
{
    Manual = 1,
    Import = 2,
    Automatic = 3,
    Realtime = 4
}

public enum EnergyReadingPlausibilityStatus
{
    Normal = 1,
    Flagged = 2
}

public sealed class EnergyReadingCaptureContext
{
    private EnergyReadingCaptureContext(string? value)
    {
        Value = value;
    }

    public string? Value { get; }

    public static EnergyReadingCaptureContext? CreateOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmedValue = value.Trim();
        Guard.AgainstMaxLength(trimmedValue, 500, nameof(value));

        return new EnergyReadingCaptureContext(trimmedValue);
    }
}

public sealed class EnergyReadingRfidTag
{
    private EnergyReadingRfidTag(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static EnergyReadingRfidTag? CreateOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmedValue = value.Trim();
        Guard.AgainstMaxLength(trimmedValue, 100, nameof(value));

        return new EnergyReadingRfidTag(trimmedValue);
    }
}

public sealed class EnergyReadingRfidExceptionReason
{
    private EnergyReadingRfidExceptionReason(string? value)
    {
        Value = value;
    }

    public string? Value { get; }

    public static EnergyReadingRfidExceptionReason? CreateOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmedValue = value.Trim();
        Guard.AgainstMaxLength(trimmedValue, 500, nameof(value));

        return new EnergyReadingRfidExceptionReason(trimmedValue);
    }
}

public sealed class EnergyReadingPlausibilityNote
{
    private EnergyReadingPlausibilityNote(string? value)
    {
        Value = value;
    }

    public string? Value { get; }

    public static EnergyReadingPlausibilityNote? CreateOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmedValue = value.Trim();
        Guard.AgainstMaxLength(trimmedValue, 500, nameof(value));

        return new EnergyReadingPlausibilityNote(trimmedValue);
    }
}

public sealed class EnergyReadingValue
{
    public EnergyReadingValue(decimal value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Reading value must not be negative.");
        }

        Value = decimal.Round(value, 3, MidpointRounding.AwayFromZero);
    }

    public decimal Value { get; }
}

public sealed class EnergyReadingDate
{
    public EnergyReadingDate(DateTime value)
    {
        Value = value;
    }

    public DateTime Value { get; }
}
