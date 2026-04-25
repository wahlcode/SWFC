namespace SWFC.Domain.M200_Business.M206_Purchasing.Suppliers;

public sealed class Supplier
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string? SupplierNumber { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private Supplier()
    {
        Name = string.Empty;
    }

    public Supplier(Guid id, string name, string? supplierNumber, DateTime createdAtUtc)
    {
        if (id == Guid.Empty) throw new ArgumentException("Id is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));

        Id = id;
        Name = name.Trim();
        SupplierNumber = string.IsNullOrWhiteSpace(supplierNumber) ? null : supplierNumber.Trim();
        IsActive = true;
        CreatedAtUtc = createdAtUtc;
    }

    public void SetActiveState(bool isActive)
    {
        IsActive = isActive;
    }
}