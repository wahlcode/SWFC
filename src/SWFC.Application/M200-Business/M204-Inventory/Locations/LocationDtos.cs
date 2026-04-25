namespace SWFC.Application.M200_Business.M204_Inventory.Locations;

public sealed record LocationDetailsDto(
    Guid Id,
    string Name,
    string Code,
    Guid? ParentLocationId,
    string? ParentLocationName,
    DateTime CreatedAtUtc,
    string CreatedBy,
    DateTime? LastModifiedAtUtc,
    string? LastModifiedBy);

public sealed record LocationLookupItem(
    Guid Id,
    string Name,
    string Code,
    Guid? ParentLocationId);

public sealed record LocationListItem(
    Guid Id,
    string Name,
    string Code,
    Guid? ParentLocationId,
    string? ParentLocationName,
    DateTime CreatedAtUtc,
    string CreatedBy,
    DateTime? LastModifiedAtUtc,
    string? LastModifiedBy);

