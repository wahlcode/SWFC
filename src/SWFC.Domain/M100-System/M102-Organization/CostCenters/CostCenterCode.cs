using SWFC.Domain.M100_System.M101_Foundation.Exceptions;

namespace SWFC.Domain.M100_System.M102_Organization.CostCenters;

public sealed class CostCenterCode
{
    private CostCenterCode(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static CostCenterCode Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException("Cost center code is required.");
        }

        var normalized = value.Trim();

        if (normalized.Length > 100)
        {
            throw new ValidationException("Cost center code must not exceed 100 characters.");
        }

        return new CostCenterCode(normalized);
    }

    public override string ToString() => Value;
}
