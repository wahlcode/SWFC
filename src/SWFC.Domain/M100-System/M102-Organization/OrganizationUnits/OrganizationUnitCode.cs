using SWFC.Domain.M100_System.M101_Foundation.Exceptions;

namespace SWFC.Domain.M100_System.M102_Organization.OrganizationUnits;

public sealed class OrganizationUnitCode
{
    private OrganizationUnitCode(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static OrganizationUnitCode Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException("Organization unit code is required.");
        }

        var normalized = value.Trim();

        if (normalized.Length > 100)
        {
            throw new ValidationException("Organization unit code must not exceed 100 characters.");
        }

        return new OrganizationUnitCode(normalized);
    }

    public override string ToString() => Value;
}

