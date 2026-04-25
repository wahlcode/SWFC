using SWFC.Domain.M100_System.M101_Foundation.Rules;

namespace SWFC.Domain.M200_Business.M205_Energy.EnergyMeters;

public enum EnergyMediumType
{
    Electricity = 1,
    Water = 2,
    Gas = 3,
    Oxygen = 4
}

public sealed class EnergyMeterName
{
    public EnergyMeterName(string value)
    {
        Guard.AgainstNullOrWhiteSpace(value, nameof(value));
        Guard.AgainstMaxLength(value, 200, nameof(value));

        Value = value.Trim();
    }

    public string Value { get; }
}

public sealed class EnergyMeterUnit
{
    public EnergyMeterUnit(string value)
    {
        Guard.AgainstNullOrWhiteSpace(value, nameof(value));
        Guard.AgainstMaxLength(value, 50, nameof(value));

        Value = value.Trim();
    }

    public string Value { get; }
}

public sealed class EnergyExternalSystem
{
    private EnergyExternalSystem(string? value)
    {
        Value = value;
    }

    public string? Value { get; }

    public static EnergyExternalSystem? CreateOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmedValue = value.Trim();
        Guard.AgainstMaxLength(trimmedValue, 100, nameof(value));

        return new EnergyExternalSystem(trimmedValue);
    }
}

public sealed class EnergyMeterRfidTag
{
    private EnergyMeterRfidTag(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static EnergyMeterRfidTag? CreateOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmedValue = value.Trim();
        Guard.AgainstMaxLength(trimmedValue, 100, nameof(value));

        return new EnergyMeterRfidTag(trimmedValue);
    }
}
