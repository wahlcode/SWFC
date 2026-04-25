namespace SWFC.Application.M200_Business.M206_Purchasing.Suppliers;

public sealed record SupplierDto(
    Guid Id,
    string Name,
    string? SupplierNumber,
    bool IsActive,
    DateTime CreatedAtUtc);

public sealed record SupplierListItem(
    Guid Id,
    string Name,
    string? SupplierNumber,
    bool IsActive,
    DateTime CreatedAtUtc);

public sealed record CreateSupplierRequest(
    string Name,
    string? SupplierNumber);
