namespace SWFC.Domain.M200_Business.M206_Purchasing.PurchaseOrders;

public sealed class PurchaseOrder
{
    public Guid Id { get; private set; }
    public string OrderNumber { get; private set; }
    public Guid SupplierId { get; private set; }
    public string? ErpReference { get; private set; }
    public string? OrderDocumentReference { get; private set; }
    public PurchaseOrderStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? OrderedAtUtc { get; private set; }

    private PurchaseOrder()
    {
        OrderNumber = string.Empty;
    }

    public PurchaseOrder(
        Guid id,
        string orderNumber,
        Guid supplierId,
        DateTime createdAtUtc,
        string? erpReference = null,
        string? orderDocumentReference = null)
    {
        if (id == Guid.Empty) throw new ArgumentException("Id is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(orderNumber)) throw new ArgumentException("Order number is required.", nameof(orderNumber));
        if (supplierId == Guid.Empty) throw new ArgumentException("Supplier id is required.", nameof(supplierId));

        Id = id;
        OrderNumber = orderNumber.Trim();
        SupplierId = supplierId;
        ErpReference = NormalizeReference(erpReference, nameof(erpReference));
        OrderDocumentReference = NormalizeReference(orderDocumentReference, nameof(orderDocumentReference));
        Status = PurchaseOrderStatus.Draft;
        CreatedAtUtc = createdAtUtc;
    }

    public void MarkOrdered(DateTime orderedAtUtc)
    {
        if (Status == PurchaseOrderStatus.Deactivated) return;

        Status = PurchaseOrderStatus.Ordered;
        OrderedAtUtc = orderedAtUtc;
    }

    public void MarkPartiallyReceived()
    {
        if (Status == PurchaseOrderStatus.Deactivated) return;
        Status = PurchaseOrderStatus.PartiallyReceived;
    }

    public void MarkReceived()
    {
        if (Status == PurchaseOrderStatus.Deactivated) return;
        Status = PurchaseOrderStatus.Received;
    }

    public void Close()
    {
        if (Status == PurchaseOrderStatus.Deactivated) return;
        Status = PurchaseOrderStatus.Closed;
    }

    public void Deactivate()
    {
        Status = PurchaseOrderStatus.Deactivated;
    }

    private static string? NormalizeReference(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();

        if (normalized.Length > 200)
        {
            throw new ArgumentException("Reference must not exceed 200 characters.", parameterName);
        }

        return normalized;
    }
}
