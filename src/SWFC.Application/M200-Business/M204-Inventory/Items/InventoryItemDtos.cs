namespace SWFC.Application.M200_Business.M204_Inventory.Items;

public sealed record InventoryItemDetailsDto(
    Guid Id,
    string ArticleNumber,
    string Name,
    string Description,
    string Unit,
    string? Barcode,
    string? Manufacturer,
    string? ManufacturerPartNumber,
    bool IsActive,
    int QuantityOnHand,
    int ReservedQuantity,
    int AvailableQuantity,
    IReadOnlyList<InventoryItemStockDto> Stocks,
    DateTime CreatedAtUtc,
    string CreatedBy,
    DateTime? LastModifiedAtUtc,
    string? LastModifiedBy);

public sealed record InventoryItemStockDto(
    Guid StockId,
    Guid LocationId,
    string LocationName,
    string LocationCode,
    string? Bin,
    int QuantityOnHand,
    int ReservedQuantity,
    int AvailableQuantity);

public sealed record InventoryItemLookupItem(
    Guid Id,
    string ArticleNumber,
    string Name,
    string Unit,
    bool IsActive);

public sealed record InventoryItemListItem(
    Guid Id,
    string ArticleNumber,
    string Name,
    string Description,
    string Unit,
    bool IsActive,
    int StockCount,
    int QuantityOnHand,
    int ReservedQuantity,
    int AvailableQuantity,
    DateTime CreatedAtUtc,
    string CreatedBy,
    DateTime? LastModifiedAtUtc,
    string? LastModifiedBy);

