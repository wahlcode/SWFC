namespace SWFC.Application.M100_System.M102_Organization.DTOs;

public sealed record OrganizationUnitListItem(
    Guid Id,
    string Name,
    string Code,
    Guid? ParentOrganizationUnitId,
    string? ParentName,
    string? ParentCode);