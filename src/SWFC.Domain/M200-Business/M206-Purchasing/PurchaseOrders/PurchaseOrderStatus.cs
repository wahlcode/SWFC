namespace SWFC.Domain.M200_Business.M206_Purchasing.PurchaseOrders;

public enum PurchaseOrderStatus
{
    Draft = 1,
    Ordered = 2,
    PartiallyReceived = 3,
    Received = 4,
    Closed = 5,
    Deactivated = 6
}