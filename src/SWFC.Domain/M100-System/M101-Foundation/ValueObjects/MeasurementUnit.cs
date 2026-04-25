using SWFC.Domain.M100_System.M101_Foundation.Rules;

namespace SWFC.Domain.M100_System.M101_Foundation.ValueObjects;

public sealed record MeasurementUnit
{
    private MeasurementUnit(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static MeasurementUnit Create(string value)
    {
        Guard.AgainstNullOrWhiteSpace(value, nameof(value));
        return new MeasurementUnit(value.Trim());
    }

    public override string ToString() => Value;
}
