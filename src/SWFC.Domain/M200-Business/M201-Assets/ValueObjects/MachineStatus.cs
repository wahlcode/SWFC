using SWFC.Domain.Common.Errors;
using SWFC.Domain.Common.Exceptions;

namespace SWFC.Domain.M200_Business.M201_Assets.ValueObjects;

public sealed class MachineStatus
{
    public const string Planned = "Planned";
    public const string Active = "Active";
    public const string InMaintenance = "InMaintenance";
    public const string Inactive = "Inactive";
    public const string Retired = "Retired";

    private static readonly HashSet<string> AllowedValues = new(StringComparer.OrdinalIgnoreCase)
    {
        Planned,
        Active,
        InMaintenance,
        Inactive,
        Retired
    };

    private MachineStatus(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static MachineStatus Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException(ErrorCodes.Validation.Invalid);
        }

        var normalized = value.Trim();

        if (!AllowedValues.Contains(normalized))
        {
            throw new ValidationException(ErrorCodes.Validation.Invalid);
        }

        return new MachineStatus(Normalize(normalized));
    }

    private static string Normalize(string value)
    {
        if (value.Equals(Planned, StringComparison.OrdinalIgnoreCase))
        {
            return Planned;
        }

        if (value.Equals(Active, StringComparison.OrdinalIgnoreCase))
        {
            return Active;
        }

        if (value.Equals(InMaintenance, StringComparison.OrdinalIgnoreCase))
        {
            return InMaintenance;
        }

        if (value.Equals(Inactive, StringComparison.OrdinalIgnoreCase))
        {
            return Inactive;
        }

        return Retired;
    }

    public override string ToString() => Value;
}