using SWFC.Domain.M200_Business.M206_Purchasing.PurchaseRequirements;

namespace SWFC.Application.M200_Business.M206_Purchasing.PurchaseRequirements;

public sealed record PurchaseRequirementDto(
    Guid Id,
    string RequiredItem,
    decimal Quantity,
    string Unit,
    PurchaseRequirementSourceType SourceType,
    Guid? SourceReferenceId,
    PurchaseRequirementStatus Status,
    DateTime CreatedAtUtc,
    DateTime? DeactivatedAtUtc);

public sealed record CreatePurchaseRequirementRequest(
    string RequiredItem,
    decimal Quantity,
    string Unit,
    PurchaseRequirementSourceType SourceType,
    Guid? SourceReferenceId);