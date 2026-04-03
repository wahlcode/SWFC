using SWFC.Domain.Common.ValueObjects;

namespace SWFC.Domain.M200_Business.M204_Inventory.Entities;

public sealed class Stock
{
    private Stock()
    {
        Id = Guid.Empty;
        InventoryItemId = Guid.Empty;
        AuditInfo = null!;
    }

    private Stock(Guid id, Guid inventoryItemId, int quantityOnHand, AuditInfo auditInfo)
    {
        Id = id;
        InventoryItemId = inventoryItemId;
        QuantityOnHand = quantityOnHand;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public Guid InventoryItemId { get; private set; }
    public int QuantityOnHand { get; private set; }
    public AuditInfo AuditInfo { get; private set; }

    public static Stock Create(
        Guid inventoryItemId,
        int quantityOnHand,
        ChangeContext changeContext)
    {
        if (inventoryItemId == Guid.Empty)
        {
            throw new ArgumentException("Inventory item id is required.", nameof(inventoryItemId));
        }

        if (quantityOnHand < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantityOnHand), "Quantity on hand must be zero or greater.");
        }

        var auditInfo = new AuditInfo(
            createdAtUtc: changeContext.ChangedAtUtc,
            createdBy: changeContext.UserId);

        return new Stock(Guid.NewGuid(), inventoryItemId, quantityOnHand, auditInfo);
    }

    public void SetQuantity(int quantityOnHand, ChangeContext changeContext)
    {
        if (quantityOnHand < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantityOnHand), "Quantity on hand must be zero or greater.");
        }

        QuantityOnHand = quantityOnHand;
        AuditInfo = new AuditInfo(
            createdAtUtc: AuditInfo.CreatedAtUtc,
            createdBy: AuditInfo.CreatedBy,
            lastModifiedAtUtc: changeContext.ChangedAtUtc,
            lastModifiedBy: changeContext.UserId);
    }
}