using SWFC.Domain.M100_System.M101_Foundation.Errors;
using SWFC.Domain.M100_System.M101_Foundation.Exceptions;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M200_Business.M204_Inventory.Items;
using SWFC.Domain.M200_Business.M204_Inventory.Locations;
using StockEntity = SWFC.Domain.M200_Business.M204_Inventory.Stock.Stock;

namespace SWFC.Domain.M200_Business.M204_Inventory.Items;

public sealed class InventoryItem
{
    private readonly List<StockEntity> _stocks = new();

    private InventoryItem()
    {
        Id = Guid.Empty;
        ArticleNumber = null!;
        Name = null!;
        Description = null!;
        Unit = null!;
        AuditInfo = null!;
    }

    private InventoryItem(
        Guid id,
        InventoryItemArticleNumber articleNumber,
        InventoryItemName name,
        InventoryItemDescription description,
        InventoryItemUnit unit,
        InventoryItemBarcode? barcode,
        InventoryItemManufacturer? manufacturer,
        InventoryItemManufacturerPartNumber? manufacturerPartNumber,
        bool isActive,
        AuditInfo auditInfo)
    {
        Id = id;
        ArticleNumber = articleNumber;
        Name = name;
        Description = description;
        Unit = unit;
        Barcode = barcode;
        Manufacturer = manufacturer;
        ManufacturerPartNumber = manufacturerPartNumber;
        IsActive = isActive;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public InventoryItemArticleNumber ArticleNumber { get; private set; }
    public InventoryItemName Name { get; private set; }
    public InventoryItemDescription Description { get; private set; }
    public InventoryItemUnit Unit { get; private set; }
    public InventoryItemBarcode? Barcode { get; private set; }
    public InventoryItemManufacturer? Manufacturer { get; private set; }
    public InventoryItemManufacturerPartNumber? ManufacturerPartNumber { get; private set; }
    public bool IsActive { get; private set; }
    public AuditInfo AuditInfo { get; private set; }

    public IReadOnlyCollection<StockEntity> Stocks => _stocks;
    public StockEntity? Stock => _stocks.Count == 1 ? _stocks[0] : null;

    public static InventoryItem Create(
        InventoryItemArticleNumber articleNumber,
        InventoryItemName name,
        InventoryItemDescription description,
        InventoryItemUnit unit,
        InventoryItemBarcode? barcode,
        InventoryItemManufacturer? manufacturer,
        InventoryItemManufacturerPartNumber? manufacturerPartNumber,
        ChangeContext changeContext)
    {
        var auditInfo = new AuditInfo(
            createdAtUtc: changeContext.ChangedAtUtc,
            createdBy: changeContext.UserId);

        return new InventoryItem(
            Guid.NewGuid(),
            articleNumber,
            name,
            description,
            unit,
            barcode,
            manufacturer,
            manufacturerPartNumber,
            isActive: true,
            auditInfo);
    }

    public void UpdateMasterData(
        InventoryItemArticleNumber articleNumber,
        InventoryItemName name,
        InventoryItemDescription description,
        InventoryItemUnit unit,
        InventoryItemBarcode? barcode,
        InventoryItemManufacturer? manufacturer,
        InventoryItemManufacturerPartNumber? manufacturerPartNumber,
        bool isActive,
        ChangeContext changeContext)
    {
        ArticleNumber = articleNumber;
        Name = name;
        Description = description;
        Unit = unit;
        Barcode = barcode;
        Manufacturer = manufacturer;
        ManufacturerPartNumber = manufacturerPartNumber;
        IsActive = isActive;

        Touch(changeContext);
    }

    public void Activate(ChangeContext changeContext)
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        Touch(changeContext);
    }

    public void Deactivate(ChangeContext changeContext)
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        Touch(changeContext);
    }

    public void AttachStock(StockEntity stock)
    {
        if (stock.InventoryItemId != Id)
        {
            throw new ValidationException(ValidationErrorCodes.Invalid);
        }

        if (_stocks.Any(x => x.Id == stock.Id))
        {
            return;
        }

        if (_stocks.Any(x => x.LocationId == stock.LocationId && string.Equals(x.Bin, stock.Bin, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ValidationException(ValidationErrorCodes.Invalid);
        }

        _stocks.Add(stock);
    }

    private void Touch(ChangeContext changeContext)
    {
        AuditInfo = new AuditInfo(
            createdAtUtc: AuditInfo.CreatedAtUtc,
            createdBy: AuditInfo.CreatedBy,
            lastModifiedAtUtc: changeContext.ChangedAtUtc,
            lastModifiedBy: changeContext.UserId);
    }
}

