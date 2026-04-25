namespace SWFC.Application.M200_Business.M206_Purchasing.GoodsReceipts;

public sealed class GetGoodsReceipts
{
    private readonly IGoodsReceiptReadRepository _goodsReceiptReadRepository;

    public GetGoodsReceipts(IGoodsReceiptReadRepository goodsReceiptReadRepository)
    {
        _goodsReceiptReadRepository = goodsReceiptReadRepository;
    }

    public Task<IReadOnlyList<GoodsReceiptListItem>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return _goodsReceiptReadRepository.GetAllAsync(cancellationToken);
    }
}
