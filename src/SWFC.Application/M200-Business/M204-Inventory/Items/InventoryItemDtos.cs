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
    decimal StandardUnitPrice,
    string Currency,
    bool IsActive,
    int QuantityOnHand,
    int ReservedQuantity,
    int AvailableQuantity,
    decimal StockValue,
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
    int AvailableQuantity,
    decimal StockValue);

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
    decimal StandardUnitPrice,
    string Currency,
    bool IsActive,
    int StockCount,
    int QuantityOnHand,
    int ReservedQuantity,
    int AvailableQuantity,
    decimal StockValue,
    DateTime CreatedAtUtc,
    string CreatedBy,
    DateTime? LastModifiedAtUtc,
    string? LastModifiedBy);

