using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;

namespace SWFC.Domain.M200_Business.M202_Maintenance.MaintenanceOrders;

public sealed class MaintenanceOrderMaterial
{
    private MaintenanceOrderMaterial()
    {
        Id = Guid.Empty;
        AuditInfo = null!;
    }

    private MaintenanceOrderMaterial(
        Guid id,
        Guid maintenanceOrderId,
        Guid itemId,
        int quantity,
        AuditInfo auditInfo)
    {
        Id = id;
        MaintenanceOrderId = maintenanceOrderId;
        ItemId = itemId;
        Quantity = quantity;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public Guid MaintenanceOrderId { get; private set; }
    public Guid ItemId { get; private set; }
    public int Quantity { get; private set; }
    public AuditInfo AuditInfo { get; private set; }

    public static MaintenanceOrderMaterial Create(
        Guid maintenanceOrderId,
        Guid itemId,
        int quantity,
        ChangeContext changeContext)
    {
        var auditInfo = new AuditInfo(
            changeContext.ChangedAtUtc,
            changeContext.UserId);

        return new MaintenanceOrderMaterial(
            Guid.NewGuid(),
            maintenanceOrderId,
            itemId,
            quantity,
            auditInfo);
    }

    public void UpdateQuantity(int quantity, ChangeContext changeContext)
    {
        Quantity = quantity;

        AuditInfo = new AuditInfo(
            AuditInfo.CreatedAtUtc,
            AuditInfo.CreatedBy,
            changeContext.ChangedAtUtc,
            changeContext.UserId);
    }
}
