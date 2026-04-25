namespace SWFC.Application.M100_System.M102_Organization.CostCenters;

public sealed record CostCenterDetailsDto(
    Guid Id,
    string Name,
    string Code,
    Guid? ParentId,
    string? ParentName,
    string? ParentCode,
    bool IsActive);

public sealed record CostCenterListItem(
    Guid Id,
    string Name,
    string Code,
    Guid? ParentId,
    string? ParentName,
    string? ParentCode,
    bool IsActive);

public sealed record CostCenterSelectionOptionDto(
    Guid Id,
    string Name,
    string Code);
