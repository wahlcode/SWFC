using SWFC.Domain.Common.ValueObjects;
using SWFC.Domain.M200_Business.M204_Inventory.ValueObjects;

namespace SWFC.Domain.M200_Business.M204_Inventory.Entities;

public sealed class InventoryItem
{
    private InventoryItem()
    {
        Id = Guid.Empty;
        Name = null!;
        AuditInfo = null!;
    }

    private InventoryItem(Guid id, InventoryItemName name, AuditInfo auditInfo)
    {
        Id = id;
        Name = name;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public InventoryItemName Name { get; private set; }
    public AuditInfo AuditInfo { get; private set; }

    public Stock? Stock { get; private set; }

    public static InventoryItem Create(InventoryItemName name, ChangeContext changeContext)
    {
        var auditInfo = new AuditInfo(
            createdAtUtc: changeContext.ChangedAtUtc,
            createdBy: changeContext.UserId);

        return new InventoryItem(Guid.NewGuid(), name, auditInfo);
    }

    public void Rename(InventoryItemName name, ChangeContext changeContext)
    {
        Name = name;
        AuditInfo = new AuditInfo(
            createdAtUtc: AuditInfo.CreatedAtUtc,
            createdBy: AuditInfo.CreatedBy,
            lastModifiedAtUtc: changeContext.ChangedAtUtc,
            lastModifiedBy: changeContext.UserId);
    }

    public void AttachStock(Stock stock)
    {
        if (stock.InventoryItemId != Id)
        {
            throw new InvalidOperationException("Stock does not belong to this inventory item.");
        }

        Stock = stock;
    }
}