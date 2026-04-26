namespace SWFC.Domain.M200_Business.M206_Purchasing.RequestForQuotations;

public sealed class RequestForQuotation
{
    public Guid Id { get; private set; }
    public Guid PurchaseRequirementId { get; private set; }
    public Guid SupplierId { get; private set; }
    public DateTime RequestedAtUtc { get; private set; }
    public DateTime? ResponseDueAtUtc { get; private set; }
    public string? OfferDocumentReference { get; private set; }
    public bool IsClosed { get; private set; }

    private RequestForQuotation()
    {
    }

    public RequestForQuotation(
        Guid id,
        Guid purchaseRequirementId,
        Guid supplierId,
        DateTime requestedAtUtc,
        DateTime? responseDueAtUtc,
        string? offerDocumentReference = null)
    {
        if (id == Guid.Empty) throw new ArgumentException("Id is required.", nameof(id));
        if (purchaseRequirementId == Guid.Empty) throw new ArgumentException("Purchase requirement id is required.", nameof(purchaseRequirementId));
        if (supplierId == Guid.Empty) throw new ArgumentException("Supplier id is required.", nameof(supplierId));

        Id = id;
        PurchaseRequirementId = purchaseRequirementId;
        SupplierId = supplierId;
        RequestedAtUtc = requestedAtUtc;
        ResponseDueAtUtc = responseDueAtUtc;
        OfferDocumentReference = NormalizeReference(offerDocumentReference, nameof(offerDocumentReference));
        IsClosed = false;
    }

    public void Close()
    {
        IsClosed = true;
    }

    private static string? NormalizeReference(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();

        if (normalized.Length > 200)
        {
            throw new ArgumentException("Reference must not exceed 200 characters.", parameterName);
        }

        return normalized;
    }
}
