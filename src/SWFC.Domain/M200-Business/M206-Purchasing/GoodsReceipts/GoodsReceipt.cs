namespace SWFC.Domain.M200_Business.M206_Purchasing.GoodsReceipts;

public sealed class GoodsReceipt
{
    public Guid Id { get; private set; }
    public Guid PurchaseOrderId { get; private set; }
    public Guid InventoryItemId { get; private set; }
    public Guid LocationId { get; private set; }
    public string? Bin { get; private set; }
    public int Quantity { get; private set; }
    public string Unit { get; private set; }
    public DateTime ReceivedAtUtc { get; private set; }
    public string? DeliveryDocumentReference { get; private set; }
    public GoodsReceiptInventoryBookingStatus InventoryBookingStatus { get; private set; }
    public Guid? InventoryStockMovementId { get; private set; }
    public string? InventoryBookingMessage { get; private set; }

    private GoodsReceipt()
    {
        Unit = string.Empty;
    }

    public GoodsReceipt(
        Guid id,
        Guid purchaseOrderId,
        Guid inventoryItemId,
        Guid locationId,
        string? bin,
        int quantity,
        string unit,
        DateTime receivedAtUtc,
        string? deliveryDocumentReference = null)
    {
        if (id == Guid.Empty) throw new ArgumentException("Id is required.", nameof(id));
        if (purchaseOrderId == Guid.Empty) throw new ArgumentException("Purchase order id is required.", nameof(purchaseOrderId));
        if (inventoryItemId == Guid.Empty) throw new ArgumentException("Inventory item id is required.", nameof(inventoryItemId));
        if (locationId == Guid.Empty) throw new ArgumentException("Location id is required.", nameof(locationId));
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        if (string.IsNullOrWhiteSpace(unit)) throw new ArgumentException("Unit is required.", nameof(unit));

        Id = id;
        PurchaseOrderId = purchaseOrderId;
        InventoryItemId = inventoryItemId;
        LocationId = locationId;
        Bin = NormalizeBin(bin);
        Quantity = quantity;
        Unit = unit.Trim();
        ReceivedAtUtc = receivedAtUtc;
        DeliveryDocumentReference = NormalizeReference(deliveryDocumentReference, nameof(deliveryDocumentReference));
        InventoryBookingStatus = GoodsReceiptInventoryBookingStatus.Requested;
    }

    public void MarkInventoryBooked(Guid stockMovementId)
    {
        if (stockMovementId == Guid.Empty) throw new ArgumentException("Stock movement id is required.", nameof(stockMovementId));

        InventoryBookingStatus = GoodsReceiptInventoryBookingStatus.Booked;
        InventoryStockMovementId = stockMovementId;
        InventoryBookingMessage = null;
    }

    public void MarkInventoryBookingFailed(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Message is required.", nameof(message));

        InventoryBookingStatus = GoodsReceiptInventoryBookingStatus.Failed;
        InventoryStockMovementId = null;
        InventoryBookingMessage = message.Trim();
    }

    private static string? NormalizeBin(string? bin)
    {
        if (string.IsNullOrWhiteSpace(bin))
        {
            return null;
        }

        var normalized = bin.Trim();

        if (normalized.Length > 100)
        {
            throw new ArgumentException("Bin must not exceed 100 characters.", nameof(bin));
        }

        return normalized;
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
