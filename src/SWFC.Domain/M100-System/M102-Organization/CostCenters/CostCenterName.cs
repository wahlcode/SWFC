using SWFC.Domain.M100_System.M101_Foundation.Exceptions;

namespace SWFC.Domain.M100_System.M102_Organization.CostCenters;

public sealed class CostCenterName
{
    private CostCenterName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static CostCenterName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException("Cost center name is required.");
        }

        var normalized = value.Trim();

        if (normalized.Length > 200)
        {
            throw new ValidationException("Cost center name must not exceed 200 characters.");
        }

        return new CostCenterName(normalized);
    }

    public override string ToString() => Value;
}
