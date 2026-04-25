using SWFC.Domain.M100_System.M101_Foundation.Rules;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M200_Business.M202_Maintenance.MaintenanceOrders;

namespace SWFC.Domain.M200_Business.M202_Maintenance.MaintenancePlans;

public enum MaintenancePlanIntervalUnit
{
    Days = 1,
    Weeks = 2,
    Months = 3
}

public sealed class MaintenancePlanName
{
    public MaintenancePlanName(string value)
    {
        Guard.AgainstNullOrWhiteSpace(value, nameof(value));
        Guard.AgainstMaxLength(value, 200, nameof(value));

        Value = value.Trim();
    }

    public string Value { get; }
}

public sealed class MaintenancePlanDescription
{
    public MaintenancePlanDescription(string value)
    {
        Guard.AgainstNullOrWhiteSpace(value, nameof(value));
        Guard.AgainstMaxLength(value, 2000, nameof(value));

        Value = value.Trim();
    }

    public string Value { get; }
}

public sealed class MaintenancePlan
{
    private MaintenancePlan()
    {
        Id = Guid.Empty;
        Name = null!;
        Description = null!;
        AuditInfo = null!;
    }

    private MaintenancePlan(
        Guid id,
        MaintenancePlanName name,
        MaintenancePlanDescription description,
        MaintenanceTargetType targetType,
        Guid targetId,
        int intervalValue,
        MaintenancePlanIntervalUnit intervalUnit,
        DateTime nextDueAtUtc,
        bool isActive,
        AuditInfo auditInfo)
    {
        Id = id;
        Name = name;
        Description = description;
        TargetType = targetType;
        TargetId = targetId;
        IntervalValue = intervalValue;
        IntervalUnit = intervalUnit;
        NextDueAtUtc = nextDueAtUtc;
        IsActive = isActive;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public MaintenancePlanName Name { get; private set; }
    public MaintenancePlanDescription Description { get; private set; }
    public MaintenanceTargetType TargetType { get; private set; }
    public Guid TargetId { get; private set; }
    public int IntervalValue { get; private set; }
    public MaintenancePlanIntervalUnit IntervalUnit { get; private set; }
    public DateTime NextDueAtUtc { get; private set; }
    public bool IsActive { get; private set; }
    public AuditInfo AuditInfo { get; private set; }

    public static MaintenancePlan Create(
        MaintenancePlanName name,
        MaintenancePlanDescription description,
        MaintenanceTargetType targetType,
        Guid targetId,
        int intervalValue,
        MaintenancePlanIntervalUnit intervalUnit,
        DateTime nextDueAtUtc,
        ChangeContext changeContext)
    {
        if (targetId == Guid.Empty)
        {
            throw new ArgumentException("Target id must not be empty.", nameof(targetId));
        }

        if (intervalValue <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(intervalValue), "Interval must be greater than zero.");
        }

        var auditInfo = new AuditInfo(
            changeContext.ChangedAtUtc,
            changeContext.UserId);

        return new MaintenancePlan(
            Guid.NewGuid(),
            name,
            description,
            targetType,
            targetId,
            intervalValue,
            intervalUnit,
            nextDueAtUtc,
            true,
            auditInfo);
    }

    public void Update(
        MaintenancePlanName name,
        MaintenancePlanDescription description,
        MaintenanceTargetType targetType,
        Guid targetId,
        int intervalValue,
        MaintenancePlanIntervalUnit intervalUnit,
        DateTime nextDueAtUtc,
        ChangeContext changeContext)
    {
        if (targetId == Guid.Empty)
        {
            throw new ArgumentException("Target id must not be empty.", nameof(targetId));
        }

        if (intervalValue <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(intervalValue), "Interval must be greater than zero.");
        }

        Name = name;
        Description = description;
        TargetType = targetType;
        TargetId = targetId;
        IntervalValue = intervalValue;
        IntervalUnit = intervalUnit;
        NextDueAtUtc = nextDueAtUtc;

        Touch(changeContext);
    }

    public void SetActiveState(bool isActive, ChangeContext changeContext)
    {
        if (IsActive == isActive)
        {
            return;
        }

        IsActive = isActive;
        Touch(changeContext);
    }

    public void ShiftNextDueDate(DateTime nextDueAtUtc, ChangeContext changeContext)
    {
        NextDueAtUtc = nextDueAtUtc;
        Touch(changeContext);
    }

    private void Touch(ChangeContext changeContext)
    {
        AuditInfo = new AuditInfo(
            AuditInfo.CreatedAtUtc,
            AuditInfo.CreatedBy,
            changeContext.ChangedAtUtc,
            changeContext.UserId);
    }
}
