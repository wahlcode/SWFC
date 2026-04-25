using SWFC.Domain.M200_Business.M206_Purchasing.GoodsReceipts;

namespace SWFC.Application.M200_Business.M206_Purchasing.GoodsReceipts;

public sealed record GoodsReceiptDto(
    Guid Id,
    Guid PurchaseOrderId,
    Guid InventoryItemId,
    Guid LocationId,
    string? Bin,
    int Quantity,
    string Unit,
    DateTime ReceivedAtUtc,
    GoodsReceiptInventoryBookingStatus InventoryBookingStatus,
    Guid? InventoryStockMovementId,
    string? InventoryBookingMessage);

public sealed record GoodsReceiptListItem(
    Guid Id,
    Guid PurchaseOrderId,
    string PurchaseOrderNumber,
    Guid InventoryItemId,
    string InventoryItemName,
    Guid LocationId,
    string LocationName,
    string LocationCode,
    string? Bin,
    int Quantity,
    string Unit,
    DateTime ReceivedAtUtc,
    GoodsReceiptInventoryBookingStatus InventoryBookingStatus,
    Guid? InventoryStockMovementId,
    string? InventoryBookingMessage);

public sealed record CreateGoodsReceiptRequest(
    Guid PurchaseOrderId,
    Guid InventoryItemId,
    Guid LocationId,
    string? Bin,
    int Quantity,
    string Unit);
