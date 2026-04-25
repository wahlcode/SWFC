namespace SWFC.Application.M200_Business.M206_Purchasing.PurchaseOrders;

public sealed class GetPurchaseOrders
{
    private readonly IPurchaseOrderReadRepository _purchaseOrderReadRepository;

    public GetPurchaseOrders(IPurchaseOrderReadRepository purchaseOrderReadRepository)
    {
        _purchaseOrderReadRepository = purchaseOrderReadRepository;
    }

    public Task<IReadOnlyList<PurchaseOrderListItem>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return _purchaseOrderReadRepository.GetAllAsync(cancellationToken);
    }
}
