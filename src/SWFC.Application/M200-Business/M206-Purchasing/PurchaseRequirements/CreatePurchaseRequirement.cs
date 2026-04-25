using SWFC.Domain.M200_Business.M206_Purchasing.PurchaseRequirements;

namespace SWFC.Application.M200_Business.M206_Purchasing.PurchaseRequirements;

public interface IPurchaseRequirementWriteRepository
{
    Task AddAsync(PurchaseRequirement purchaseRequirement, CancellationToken cancellationToken = default);
}

public sealed class CreatePurchaseRequirement
{
    private readonly IPurchaseRequirementWriteRepository _purchaseRequirementWriteRepository;

    public CreatePurchaseRequirement(IPurchaseRequirementWriteRepository purchaseRequirementWriteRepository)
    {
        _purchaseRequirementWriteRepository = purchaseRequirementWriteRepository;
    }

    public async Task<PurchaseRequirementDto> ExecuteAsync(
        CreatePurchaseRequirementRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var purchaseRequirement = new PurchaseRequirement(
            Guid.NewGuid(),
            request.RequiredItem,
            request.Quantity,
            request.Unit,
            request.SourceType,
            request.SourceReferenceId,
            DateTime.UtcNow);

        await _purchaseRequirementWriteRepository.AddAsync(purchaseRequirement, cancellationToken);

        return new PurchaseRequirementDto(
            purchaseRequirement.Id,
            purchaseRequirement.RequiredItem,
            purchaseRequirement.Quantity,
            purchaseRequirement.Unit,
            purchaseRequirement.SourceType,
            purchaseRequirement.SourceReferenceId,
            purchaseRequirement.Status,
            purchaseRequirement.CreatedAtUtc,
            purchaseRequirement.DeactivatedAtUtc);
    }
}