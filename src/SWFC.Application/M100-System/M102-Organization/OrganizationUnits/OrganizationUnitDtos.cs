namespace SWFC.Application.M100_System.M102_Organization.OrganizationUnits;

public sealed record OrganizationUnitDetailsDto(
    Guid Id,
    string Name,
    string Code,
    Guid? ParentId,
    string? ParentName,
    string? ParentCode,
    bool IsActive);

public sealed record OrganizationUnitListItem(
    Guid Id,
    string Name,
    string Code,
    Guid? ParentId,
    string? ParentName,
    string? ParentCode,
    bool IsActive);

public sealed record OrganizationUnitReference(
    Guid Id,
    string Name,
    string Code,
    bool IsPrimary);

public sealed record OrganizationUnitSelectionOptionDto(
    Guid Id,
    string Name,
    string Code,
    bool IsActive);
