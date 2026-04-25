using SWFC.Domain.M200_Business.M206_Purchasing.PurchaseOrders;

namespace SWFC.Application.M200_Business.M206_Purchasing.PurchaseOrders;

public interface IPurchaseOrderWriteRepository
{
    Task AddAsync(PurchaseOrder purchaseOrder, CancellationToken cancellationToken = default);
}

public interface IPurchaseOrderReadRepository
{
    Task<IReadOnlyList<PurchaseOrderListItem>> GetAllAsync(CancellationToken cancellationToken = default);
}

public sealed class CreatePurchaseOrder
{
    private readonly IPurchaseOrderWriteRepository _purchaseOrderWriteRepository;

    public CreatePurchaseOrder(IPurchaseOrderWriteRepository purchaseOrderWriteRepository)
    {
        _purchaseOrderWriteRepository = purchaseOrderWriteRepository;
    }

    public async Task<PurchaseOrderDto> ExecuteAsync(
        CreatePurchaseOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var purchaseOrder = new PurchaseOrder(
            Guid.NewGuid(),
            request.OrderNumber,
            request.SupplierId,
            DateTime.UtcNow);

        await _purchaseOrderWriteRepository.AddAsync(purchaseOrder, cancellationToken);

        return new PurchaseOrderDto(
            purchaseOrder.Id,
            purchaseOrder.OrderNumber,
            purchaseOrder.SupplierId,
            purchaseOrder.Status,
            purchaseOrder.CreatedAtUtc,
            purchaseOrder.OrderedAtUtc);
    }
}
