namespace SWFC.Domain.M200_Business.M206_Purchasing.PurchaseRequirements;

public sealed class PurchaseRequirement
{
    public Guid Id { get; private set; }
    public string RequiredItem { get; private set; }
    public decimal Quantity { get; private set; }
    public string Unit { get; private set; }
    public PurchaseRequirementSourceType SourceType { get; private set; }
    public Guid? SourceReferenceId { get; private set; }
    public PurchaseRequirementStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? DeactivatedAtUtc { get; private set; }

    private PurchaseRequirement()
    {
        RequiredItem = string.Empty;
        Unit = string.Empty;
    }

    public PurchaseRequirement(
        Guid id,
        string requiredItem,
        decimal quantity,
        string unit,
        PurchaseRequirementSourceType sourceType,
        Guid? sourceReferenceId,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty) throw new ArgumentException("Id is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(requiredItem)) throw new ArgumentException("Required item is required.", nameof(requiredItem));
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        if (string.IsNullOrWhiteSpace(unit)) throw new ArgumentException("Unit is required.", nameof(unit));

        Id = id;
        RequiredItem = requiredItem.Trim();
        Quantity = quantity;
        Unit = unit.Trim();
        SourceType = sourceType;
        SourceReferenceId = sourceReferenceId;
        Status = PurchaseRequirementStatus.Open;
        CreatedAtUtc = createdAtUtc;
    }

    public void MarkOrdered()
    {
        if (Status == PurchaseRequirementStatus.Deactivated) return;
        Status = PurchaseRequirementStatus.Ordered;
    }

    public void Close()
    {
        if (Status == PurchaseRequirementStatus.Deactivated) return;
        Status = PurchaseRequirementStatus.Closed;
    }

    public void Deactivate(DateTime deactivatedAtUtc)
    {
        Status = PurchaseRequirementStatus.Deactivated;
        DeactivatedAtUtc = deactivatedAtUtc;
    }
}