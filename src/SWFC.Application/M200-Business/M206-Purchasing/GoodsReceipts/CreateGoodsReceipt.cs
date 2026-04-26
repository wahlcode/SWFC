using SWFC.Application.M200_Business.M204_Inventory.Stock;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Pipeline.Execution;
using SWFC.Domain.M200_Business.M204_Inventory.Reservations;
using SWFC.Domain.M200_Business.M204_Inventory.Stock;
using SWFC.Domain.M200_Business.M206_Purchasing.GoodsReceipts;

namespace SWFC.Application.M200_Business.M206_Purchasing.GoodsReceipts;

public interface IGoodsReceiptWriteRepository
{
    Task AddAsync(GoodsReceipt goodsReceipt, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IGoodsReceiptReadRepository
{
    Task<IReadOnlyList<GoodsReceiptListItem>> GetAllAsync(CancellationToken cancellationToken = default);
}

public sealed class CreateGoodsReceipt
{
    private readonly IGoodsReceiptWriteRepository _goodsReceiptWriteRepository;
    private readonly IExecutionPipeline<CreateStockMovementCommand, Guid> _stockMovementPipeline;

    public CreateGoodsReceipt(
        IGoodsReceiptWriteRepository goodsReceiptWriteRepository,
        IExecutionPipeline<CreateStockMovementCommand, Guid> stockMovementPipeline)
    {
        _goodsReceiptWriteRepository = goodsReceiptWriteRepository;
        _stockMovementPipeline = stockMovementPipeline;
    }

    public async Task<GoodsReceiptDto> ExecuteAsync(
        CreateGoodsReceiptRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var goodsReceipt = new GoodsReceipt(
            Guid.NewGuid(),
            request.PurchaseOrderId,
            request.InventoryItemId,
            request.LocationId,
            request.Bin,
            request.Quantity,
            request.Unit,
            DateTime.UtcNow,
            request.DeliveryDocumentReference);

        await _goodsReceiptWriteRepository.AddAsync(goodsReceipt, cancellationToken);
        await _goodsReceiptWriteRepository.SaveChangesAsync(cancellationToken);

        try
        {
            var movementResult = await _stockMovementPipeline.ExecuteAsync(
                new CreateStockMovementCommand(
                    request.InventoryItemId,
                    request.LocationId,
                    request.Bin,
                    StockMovementType.GoodsReceipt,
                    request.Quantity,
                    InventoryTargetType.GoodsReceipt,
                    goodsReceipt.Id.ToString(),
                    $"M206 Wareneingang fuer Bestellung {request.PurchaseOrderId}"),
                cancellationToken);

            if (movementResult.IsSuccess && movementResult.Value != Guid.Empty)
            {
                goodsReceipt.MarkInventoryBooked(movementResult.Value);
            }
            else
            {
                goodsReceipt.MarkInventoryBookingFailed(
                    string.IsNullOrWhiteSpace(movementResult.Error.Message)
                        ? "M204 stock booking did not return a stock movement id."
                        : movementResult.Error.Message);
            }
        }
        catch (Exception exception)
        {
            goodsReceipt.MarkInventoryBookingFailed(exception.Message);
        }

        await _goodsReceiptWriteRepository.SaveChangesAsync(cancellationToken);

        return new GoodsReceiptDto(
            goodsReceipt.Id,
            goodsReceipt.PurchaseOrderId,
            goodsReceipt.InventoryItemId,
            goodsReceipt.LocationId,
            goodsReceipt.Bin,
            goodsReceipt.Quantity,
            goodsReceipt.Unit,
            goodsReceipt.ReceivedAtUtc,
            goodsReceipt.DeliveryDocumentReference,
            goodsReceipt.InventoryBookingStatus,
            goodsReceipt.InventoryStockMovementId,
            goodsReceipt.InventoryBookingMessage);
    }
}
