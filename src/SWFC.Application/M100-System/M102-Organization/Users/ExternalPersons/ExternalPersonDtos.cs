namespace SWFC.Application.M100_System.M102_Organization.Users.ExternalPersons;

public sealed record ExternalPersonDetailsDto(
    Guid Id,
    string DisplayName,
    string CompanyName,
    string? Email,
    string? Phone,
    string? Function,
    Guid? OrganizationUnitId,
    bool IsActive);

public sealed record ExternalPersonListItem(
    Guid Id,
    string DisplayName,
    string CompanyName,
    string? Email,
    string? Phone,
    string? Function,
    Guid? OrganizationUnitId,
    bool IsActive);