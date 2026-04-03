namespace SWFC.Application.M200_Business.M204_Inventory.Queries;

public sealed record GetInventoryItemByIdQuery(Guid Id);

public sealed record InventoryItemDetailsDto(
    Guid Id,
    string Name,
    int QuantityOnHand,
    DateTime CreatedAtUtc,
    string CreatedBy,
    DateTime? LastModifiedAtUtc,
    string? LastModifiedBy);