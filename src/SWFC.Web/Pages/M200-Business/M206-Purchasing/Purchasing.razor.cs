using SWFC.Application.M200_Business.M204_Inventory.Items;
using SWFC.Application.M200_Business.M204_Inventory.Locations;
using SWFC.Application.M200_Business.M206_Purchasing.GoodsReceipts;
using SWFC.Application.M200_Business.M206_Purchasing.PurchaseOrders;
using SWFC.Application.M200_Business.M206_Purchasing.PurchaseRequirements;
using SWFC.Application.M200_Business.M206_Purchasing.RequestForQuotations;
using SWFC.Application.M200_Business.M206_Purchasing.Suppliers;
using SWFC.Domain.M200_Business.M206_Purchasing.GoodsReceipts;
using SWFC.Domain.M200_Business.M206_Purchasing.PurchaseRequirements;

namespace SWFC.Web.Pages.M200_Business.M206_Purchasing;

public partial class Purchasing
{
    private bool _isLoading = true;
    private bool _isBusy;
    private string? _message;
    private string? _error;

    private IReadOnlyList<PurchaseRequirementDto> _requirements = [];
    private IReadOnlyList<PurchaseProposalListItem> _proposals = [];
    private IReadOnlyList<SupplierListItem> _suppliers = [];
    private IReadOnlyList<PurchaseOrderListItem> _orders = [];
    private IReadOnlyList<GoodsReceiptListItem> _goodsReceipts = [];
    private IReadOnlyList<RequestForQuotationListItem> _rfqs = [];
    private IReadOnlyList<InventoryItemLookupItem> _inventoryItems = [];
    private IReadOnlyList<LocationLookupItem> _locations = [];

    private string _requirementItem = string.Empty;
    private decimal _requirementQuantity = 1;
    private string _requirementUnit = "Stk";
    private PurchaseRequirementSourceType _requirementSourceType = PurchaseRequirementSourceType.Manual;
    private string? _requirementSourceReference;
    private string _supplierName = string.Empty;
    private string? _supplierNumber;
    private string _orderNumber = string.Empty;
    private string _orderSupplierId = string.Empty;
    private string? _orderErpReference;
    private string? _orderDocumentReference;
    private string _rfqRequirementId = string.Empty;
    private string _rfqSupplierId = string.Empty;
    private string? _rfqOfferDocumentReference;
    private string _receiptOrderId = string.Empty;
    private string _receiptInventoryItemId = string.Empty;
    private string _receiptLocationId = string.Empty;
    private string? _receiptBin;
    private string? _receiptDeliveryDocumentReference;
    private int _receiptQuantity = 1;

    private string ReceiptUnit =>
        _inventoryItems.FirstOrDefault(x => x.Id.ToString() == _receiptInventoryItemId)?.Unit ?? "-";

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        _isLoading = true;
        _error = null;

        try
        {
            _requirements = await GetPurchaseRequirementsUseCase.ExecuteAsync();
            _proposals = await GetPurchaseProposalsUseCase.ExecuteAsync();
            _suppliers = await GetSuppliersUseCase.ExecuteAsync();
            _orders = await GetPurchaseOrdersUseCase.ExecuteAsync();
            _goodsReceipts = await GetGoodsReceiptsUseCase.ExecuteAsync();
            _rfqs = await GetRequestForQuotationsUseCase.ExecuteAsync();

            var itemResult = await InventoryItemsPipeline.ExecuteAsync(new GetInventoryItemLookupQuery());
            var locationResult = await LocationsPipeline.ExecuteAsync(new GetLocationLookupQuery());

            _inventoryItems = itemResult.IsSuccess && itemResult.Value is not null ? itemResult.Value : [];
            _locations = locationResult.IsSuccess && locationResult.Value is not null ? locationResult.Value : [];

            if (!itemResult.IsSuccess || !locationResult.IsSuccess)
            {
                _error = "M204-Auswahldaten konnten nicht geladen werden.";
            }
        }
        catch (Exception exception)
        {
            _error = exception.Message;
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task CreateRequirementAsync()
    {
        await RunAndReloadAsync(async () =>
        {
            await CreatePurchaseRequirementUseCase.ExecuteAsync(
                new CreatePurchaseRequirementRequest(
                    _requirementItem,
                    _requirementQuantity,
                    _requirementUnit,
                    _requirementSourceType,
                    ParseOptionalGuid(_requirementSourceReference, "Quellreferenz muss eine gueltige ID sein.")));

            _requirementItem = string.Empty;
            _requirementQuantity = 1;
            _requirementUnit = "Stk";
            _requirementSourceType = PurchaseRequirementSourceType.Manual;
            _requirementSourceReference = null;
            _message = "Bedarf wurde gespeichert.";
        });
    }

    private async Task CreateSupplierAsync()
    {
        await RunAndReloadAsync(async () =>
        {
            await CreateSupplierUseCase.ExecuteAsync(new CreateSupplierRequest(_supplierName, _supplierNumber));
            _supplierName = string.Empty;
            _supplierNumber = null;
            _message = "Lieferant wurde gespeichert.";
        });
    }

    private async Task CreateOrderAsync()
    {
        await RunAndReloadAsync(async () =>
        {
            var supplierId = RequireGuid(_orderSupplierId, "Lieferant ist erforderlich.");
            await CreatePurchaseOrderUseCase.ExecuteAsync(new CreatePurchaseOrderRequest(
                _orderNumber,
                supplierId,
                _orderErpReference,
                _orderDocumentReference));
            _orderNumber = string.Empty;
            _orderSupplierId = string.Empty;
            _orderErpReference = null;
            _orderDocumentReference = null;
            _message = "Bestellung wurde gespeichert.";
        });
    }

    private async Task CreateRfqAsync()
    {
        await RunAndReloadAsync(async () =>
        {
            var requirementId = RequireGuid(_rfqRequirementId, "Bedarf ist erforderlich.");
            var supplierId = RequireGuid(_rfqSupplierId, "Lieferant ist erforderlich.");
            await CreateRequestForQuotationUseCase.ExecuteAsync(new CreateRequestForQuotationRequest(
                requirementId,
                supplierId,
                null,
                _rfqOfferDocumentReference));
            _rfqRequirementId = string.Empty;
            _rfqSupplierId = string.Empty;
            _rfqOfferDocumentReference = null;
            _message = "Angebotsanfrage wurde gespeichert.";
        });
    }

    private async Task CreateReceiptAsync()
    {
        await RunAndReloadAsync(async () =>
        {
            var orderId = RequireGuid(_receiptOrderId, "Bestellung ist erforderlich.");
            var itemId = RequireGuid(_receiptInventoryItemId, "M204-Artikel ist erforderlich.");
            var locationId = RequireGuid(_receiptLocationId, "Lagerort ist erforderlich.");
            var unit = ReceiptUnit;

            if (unit == "-")
            {
                throw new InvalidOperationException("Einheit konnte nicht aus M204 ermittelt werden.");
            }

            var receipt = await CreateGoodsReceiptUseCase.ExecuteAsync(
                new CreateGoodsReceiptRequest(
                    orderId,
                    itemId,
                    locationId,
                    _receiptBin,
                    _receiptQuantity,
                    unit,
                    _receiptDeliveryDocumentReference));

            _receiptOrderId = string.Empty;
            _receiptInventoryItemId = string.Empty;
            _receiptLocationId = string.Empty;
            _receiptBin = null;
            _receiptDeliveryDocumentReference = null;
            _receiptQuantity = 1;
            _message = receipt.InventoryBookingStatus == GoodsReceiptInventoryBookingStatus.Booked
                ? "Wareneingang wurde erfasst und durch M204 gebucht."
                : "Wareneingang wurde erfasst; M204-Buchung ist fehlgeschlagen.";
        });
    }

    private async Task RunAndReloadAsync(Func<Task> action)
    {
        _isBusy = true;
        _message = null;
        _error = null;

        try
        {
            await action();
            await LoadAsync();
        }
        catch (Exception exception)
        {
            _error = exception.Message;
        }
        finally
        {
            _isBusy = false;
        }
    }

    private static Guid RequireGuid(string value, string message)
    {
        return Guid.TryParse(value, out var id) && id != Guid.Empty
            ? id
            : throw new InvalidOperationException(message);
    }

    private static Guid? ParseOptionalGuid(string? value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Guid.TryParse(value, out var id) && id != Guid.Empty
            ? id
            : throw new InvalidOperationException(message);
    }

    private static string FormatDate(DateTime value)
    {
        return value.ToString("yyyy-MM-dd HH:mm");
    }

    private static string BookingStatusClass(GoodsReceiptInventoryBookingStatus status)
    {
        return status == GoodsReceiptInventoryBookingStatus.Booked
            ? "purchasing-status purchasing-status--booked"
            : status == GoodsReceiptInventoryBookingStatus.Failed
                ? "purchasing-status purchasing-status--failed"
                : "purchasing-status";
    }
}
