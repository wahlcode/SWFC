using SWFC.Domain.M200_Business.M206_Purchasing.PurchaseOrders;

namespace SWFC.Application.M200_Business.M206_Purchasing.PurchaseOrders;

public sealed record PurchaseOrderDto(
    Guid Id,
    string OrderNumber,
    Guid SupplierId,
    PurchaseOrderStatus Status,
    DateTime CreatedAtUtc,
    DateTime? OrderedAtUtc);

public sealed record PurchaseOrderListItem(
    Guid Id,
    string OrderNumber,
    Guid SupplierId,
    string SupplierName,
    PurchaseOrderStatus Status,
    DateTime CreatedAtUtc,
    DateTime? OrderedAtUtc);

public sealed record CreatePurchaseOrderRequest(
    string OrderNumber,
    Guid SupplierId);
