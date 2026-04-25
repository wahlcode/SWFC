using SWFC.Domain.M100_System.M101_Foundation.Rules;

namespace SWFC.Domain.M200_Business.M202_Maintenance.MaintenanceOrders;

public sealed class MaintenanceOrderNumber
{
    public MaintenanceOrderNumber(string value)
    {
        Guard.AgainstNullOrWhiteSpace(value, nameof(value));
        Guard.AgainstMaxLength(value, 50, nameof(value));

        Value = value.Trim();
    }

    public string Value { get; }
}

public sealed class MaintenanceOrderTitle
{
    public MaintenanceOrderTitle(string value)
    {
        Guard.AgainstNullOrWhiteSpace(value, nameof(value));
        Guard.AgainstMaxLength(value, 200, nameof(value));

        Value = value.Trim();
    }

    public string Value { get; }
}

public sealed class MaintenanceOrderDescription
{
    public MaintenanceOrderDescription(string value)
    {
        Guard.AgainstNullOrWhiteSpace(value, nameof(value));
        Guard.AgainstMaxLength(value, 2000, nameof(value));

        Value = value.Trim();
    }

    public string Value { get; }
}
