namespace SWFC.Application.M200_Business.M204_Inventory.Queries;

public sealed record GetInventoryItemByIdQuery(Guid Id);

public sealed record InventoryItemDetailsDto(
    Guid Id,
    string Name,
    DateTime CreatedAtUtc,
    string CreatedBy,
    DateTime? LastModifiedAtUtc,
    string? LastModifiedBy);