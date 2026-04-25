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
        MaintenanceTargetType targetType,
        Guid targetId,
        AuditInfo auditInfo)
    {
        Id = id;
        Number = number;
        Title = title;
        Description = description;
        Type = type;
        Status = status;
        TargetType = targetType;
        TargetId = targetId;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public MaintenanceOrderNumber Number { get; private set; }
    public MaintenanceOrderTitle Title { get; private set; }
    public MaintenanceOrderDescription Description { get; private set; }
    public MaintenanceOrderType Type { get; private set; }
    public MaintenanceOrderStatus Status { get; private set; }
    public MaintenanceTargetType TargetType { get; private set; }
    public Guid TargetId { get; private set; }
    public AuditInfo AuditInfo { get; private set; }

    public IReadOnlyCollection<MaintenanceOrderMaterial> Materials => _materials;

    public static MaintenanceOrder Create(
        MaintenanceOrderNumber number,
        MaintenanceOrderTitle title,
        MaintenanceOrderDescription description,
        MaintenanceOrderType type,
        MaintenanceTargetType targetType,
        Guid targetId,
        ChangeContext changeContext)
    {
        var auditInfo = new AuditInfo(
            changeContext.ChangedAtUtc,
            changeContext.UserId);

        return new MaintenanceOrder(
            Guid.NewGuid(),
            number,
            title,
            description,
            type,
            MaintenanceOrderStatus.Open,
            targetType,
            targetId,
            auditInfo);
    }

    public MaintenanceOrderMaterial AddMaterial(
        Guid itemId,
        int quantity,
        ChangeContext changeContext)
    {
        var material = MaintenanceOrderMaterial.Create(Id, itemId, quantity, changeContext);
        _materials.Add(material);

        AuditInfo = new AuditInfo(
            AuditInfo.CreatedAtUtc,
            AuditInfo.CreatedBy,
            changeContext.ChangedAtUtc,
            changeContext.UserId);

        return material;
    }
}
