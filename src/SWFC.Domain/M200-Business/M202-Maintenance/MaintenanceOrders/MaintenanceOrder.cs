using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;

namespace SWFC.Domain.M200_Business.M202_Maintenance.MaintenanceOrders;

public sealed class MaintenanceOrder
{
    private readonly List<MaintenanceOrderMaterial> _materials = new();

    private MaintenanceOrder()
    {
        Id = Guid.Empty;
        Number = null!;
        Title = null!;
        Description = null!;
        AuditInfo = null!;
    }

    private MaintenanceOrder(
        Guid id,
        MaintenanceOrderNumber number,
        MaintenanceOrderTitle title,
        MaintenanceOrderDescription description,
        MaintenanceOrderType type,
        MaintenanceOrderStatus status,
        MaintenanceOrderPriority priority,
        MaintenanceTargetType targetType,
        Guid targetId,
        Guid? maintenancePlanId,
        DateTime? plannedStartUtc,
        DateTime? plannedEndUtc,
        DateTime? startedAtUtc,
        DateTime? completedAtUtc,
        DateTime? dueAtUtc,
        AuditInfo auditInfo)
    {
        Id = id;
        Number = number;
        Title = title;
        Description = description;
        Type = type;
        Status = status;
        Priority = priority;
        TargetType = targetType;
        TargetId = targetId;
        MaintenancePlanId = maintenancePlanId;
        PlannedStartUtc = plannedStartUtc;
        PlannedEndUtc = plannedEndUtc;
        StartedAtUtc = startedAtUtc;
        CompletedAtUtc = completedAtUtc;
        DueAtUtc = dueAtUtc;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public MaintenanceOrderNumber Number { get; private set; }
    public MaintenanceOrderTitle Title { get; private set; }
    public MaintenanceOrderDescription Description { get; private set; }
    public MaintenanceOrderType Type { get; private set; }
    public MaintenanceOrderStatus Status { get; private set; }
    public MaintenanceOrderPriority Priority { get; private set; }
    public MaintenanceTargetType TargetType { get; private set; }
    public Guid TargetId { get; private set; }
    public Guid? MaintenancePlanId { get; private set; }
    public DateTime? PlannedStartUtc { get; private set; }
    public DateTime? PlannedEndUtc { get; private set; }
    public DateTime? StartedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public DateTime? DueAtUtc { get; private set; }
    public AuditInfo AuditInfo { get; private set; }

    public IReadOnlyCollection<MaintenanceOrderMaterial> Materials => _materials;

    public static MaintenanceOrder Create(
        MaintenanceOrderNumber number,
        MaintenanceOrderTitle title,
        MaintenanceOrderDescription description,
        MaintenanceOrderType type,
        MaintenanceOrderPriority priority,
        MaintenanceTargetType targetType,
        Guid targetId,
        Guid? maintenancePlanId,
        DateTime? plannedStartUtc,
        DateTime? plannedEndUtc,
        DateTime? dueAtUtc,
        ChangeContext changeContext)
    {
        ValidateTarget(targetId);
        ValidatePlan(maintenancePlanId);
        ValidatePlanningWindow(plannedStartUtc, plannedEndUtc);

        var auditInfo = new AuditInfo(
            changeContext.ChangedAtUtc,
            changeContext.UserId);

        return new MaintenanceOrder(
            Guid.NewGuid(),
            number,
            title,
            description,
            type,
            MaintenanceOrderStatus.Planned,
            priority,
            targetType,
            targetId,
            maintenancePlanId,
            plannedStartUtc,
            plannedEndUtc,
            null,
            null,
            dueAtUtc,
            auditInfo);
    }

    public void Update(
        MaintenanceOrderTitle title,
        MaintenanceOrderDescription description,
        MaintenanceOrderType type,
        MaintenanceOrderPriority priority,
        MaintenanceOrderStatus status,
        MaintenanceTargetType targetType,
        Guid targetId,
        Guid? maintenancePlanId,
        DateTime? plannedStartUtc,
        DateTime? plannedEndUtc,
        DateTime? dueAtUtc,
        IReadOnlyCollection<(Guid ItemId, int Quantity)> materials,
        ChangeContext changeContext)
    {
        ValidateTarget(targetId);
        ValidatePlan(maintenancePlanId);
        ValidatePlanningWindow(plannedStartUtc, plannedEndUtc);

        Title = title;
        Description = description;
        Type = type;
        Priority = priority;
        TargetType = targetType;
        TargetId = targetId;
        MaintenancePlanId = maintenancePlanId;
        PlannedStartUtc = plannedStartUtc;
        PlannedEndUtc = plannedEndUtc;
        DueAtUtc = dueAtUtc;

        SetStatus(status, changeContext);

        _materials.Clear();
        foreach (var material in materials)
        {
            _materials.Add(MaintenanceOrderMaterial.Create(Id, material.ItemId, material.Quantity, changeContext));
        }

        Touch(changeContext);
    }

    public MaintenanceOrderMaterial AddMaterial(
        Guid itemId,
        int quantity,
        ChangeContext changeContext)
    {
        var material = MaintenanceOrderMaterial.Create(Id, itemId, quantity, changeContext);
        _materials.Add(material);

        Touch(changeContext);

        return material;
    }

    private void SetStatus(MaintenanceOrderStatus status, ChangeContext changeContext)
    {
        if (Status == status)
        {
            return;
        }

        Status = status;

        if (status == MaintenanceOrderStatus.InProgress && StartedAtUtc is null)
        {
            StartedAtUtc = changeContext.ChangedAtUtc;
        }

        if (status == MaintenanceOrderStatus.Completed)
        {
            StartedAtUtc ??= changeContext.ChangedAtUtc;
            CompletedAtUtc = changeContext.ChangedAtUtc;
        }

        if (status is MaintenanceOrderStatus.Planned or MaintenanceOrderStatus.Cancelled)
        {
            CompletedAtUtc = null;
        }
    }

    private void Touch(ChangeContext changeContext)
    {
        AuditInfo = new AuditInfo(
            AuditInfo.CreatedAtUtc,
            AuditInfo.CreatedBy,
            changeContext.ChangedAtUtc,
            changeContext.UserId);
    }

    private static void ValidateTarget(Guid targetId)
    {
        if (targetId == Guid.Empty)
        {
            throw new ArgumentException("Target id must not be empty.", nameof(targetId));
        }
    }

    private static void ValidatePlan(Guid? maintenancePlanId)
    {
        if (maintenancePlanId == Guid.Empty)
        {
            throw new ArgumentException("Maintenance plan id must not be empty.", nameof(maintenancePlanId));
        }
    }

    private static void ValidatePlanningWindow(DateTime? plannedStartUtc, DateTime? plannedEndUtc)
    {
        if (plannedStartUtc.HasValue &&
            plannedEndUtc.HasValue &&
            plannedEndUtc.Value < plannedStartUtc.Value)
        {
            throw new ArgumentException("Planned end must not be before planned start.");
        }
    }
}
