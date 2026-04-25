using SWFC.Domain.M100_System.M101_Foundation.Exceptions;

namespace SWFC.Domain.M100_System.M102_Organization.OrganizationUnits;

public sealed class OrganizationUnitName
{
    private OrganizationUnitName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static OrganizationUnitName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException("Organization unit name is required.");
        }

        var normalized = value.Trim();

        if (normalized.Length > 200)
        {
            throw new ValidationException("Organization unit name must not exceed 200 characters.");
        }

        return new OrganizationUnitName(normalized);
    }

    public override string ToString() => Value;
}

