namespace SWFC.Application.M200_Business.M206_Purchasing.RequestForQuotations;

public sealed record RequestForQuotationDto(
    Guid Id,
    Guid PurchaseRequirementId,
    Guid SupplierId,
    DateTime RequestedAtUtc,
    DateTime? ResponseDueAtUtc,
    bool IsClosed);

public sealed record RequestForQuotationListItem(
    Guid Id,
    Guid PurchaseRequirementId,
    string RequiredItem,
    Guid SupplierId,
    string SupplierName,
    DateTime RequestedAtUtc,
    DateTime? ResponseDueAtUtc,
    bool IsClosed);

public sealed record CreateRequestForQuotationRequest(
    Guid PurchaseRequirementId,
    Guid SupplierId,
    DateTime? ResponseDueAtUtc);
