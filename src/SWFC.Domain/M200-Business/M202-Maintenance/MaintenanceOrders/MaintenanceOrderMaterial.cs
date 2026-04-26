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
        if (maintenanceOrderId == Guid.Empty)
        {
            throw new ArgumentException("Maintenance order id must not be empty.", nameof(maintenanceOrderId));
        }

        if (itemId == Guid.Empty)
        {
            throw new ArgumentException("Item id must not be empty.", nameof(itemId));
        }

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

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
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        Quantity = quantity;

        AuditInfo = new AuditInfo(
            AuditInfo.CreatedAtUtc,
            AuditInfo.CreatedBy,
            changeContext.ChangedAtUtc,
            changeContext.UserId);
    }
}
