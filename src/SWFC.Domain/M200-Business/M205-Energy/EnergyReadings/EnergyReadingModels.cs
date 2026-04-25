using SWFC.Domain.M100_System.M101_Foundation.Rules;

namespace SWFC.Domain.M200_Business.M205_Energy.EnergyReadings;

public enum EnergyReadingSource
{
    Manual = 1,
    Import = 2
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
