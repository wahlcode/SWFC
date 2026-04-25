namespace SWFC.Application.M200_Business.M206_Purchasing.PurchaseRequirements;

public interface IPurchaseRequirementReadRepository
{
    Task<IReadOnlyList<PurchaseRequirementDto>> GetAllAsync(CancellationToken cancellationToken = default);
}

public sealed class GetPurchaseRequirements
{
    private readonly IPurchaseRequirementReadRepository _purchaseRequirementReadRepository;

    public GetPurchaseRequirements(IPurchaseRequirementReadRepository purchaseRequirementReadRepository)
    {
        _purchaseRequirementReadRepository = purchaseRequirementReadRepository;
    }

    public Task<IReadOnlyList<PurchaseRequirementDto>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return _purchaseRequirementReadRepository.GetAllAsync(cancellationToken);
    }
}