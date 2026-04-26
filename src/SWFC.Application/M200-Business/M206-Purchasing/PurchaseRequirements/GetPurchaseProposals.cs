using SWFC.Domain.M200_Business.M206_Purchasing.PurchaseRequirements;

namespace SWFC.Application.M200_Business.M206_Purchasing.PurchaseRequirements;

public sealed record PurchaseProposalListItem(
    Guid PurchaseRequirementId,
    string RequiredItem,
    decimal Quantity,
    string Unit,
    PurchaseRequirementSourceType SourceType,
    bool IsAutomatic,
    Guid? SourceReferenceId);

public sealed class GetPurchaseProposals
{
    private readonly IPurchaseRequirementReadRepository _purchaseRequirementReadRepository;

    public GetPurchaseProposals(IPurchaseRequirementReadRepository purchaseRequirementReadRepository)
    {
        _purchaseRequirementReadRepository = purchaseRequirementReadRepository;
    }

    public async Task<IReadOnlyList<PurchaseProposalListItem>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var requirements = await _purchaseRequirementReadRepository.GetAllAsync(cancellationToken);

        return requirements
            .Where(x => x.Status == PurchaseRequirementStatus.Open)
            .Select(x => new PurchaseProposalListItem(
                x.Id,
                x.RequiredItem,
                x.Quantity,
                x.Unit,
                x.SourceType,
                x.SourceType != PurchaseRequirementSourceType.Manual,
                x.SourceReferenceId))
            .ToArray();
    }
}
