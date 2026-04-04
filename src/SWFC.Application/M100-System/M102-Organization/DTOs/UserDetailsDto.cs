namespace SWFC.Application.M100_System.M102_Organization.DTOs;

public sealed record UserDetailsDto(
    Guid Id,
    string IdentityKey,
    string DisplayName,
    bool IsActive,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<Guid> OrganizationUnitIds);