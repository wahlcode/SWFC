namespace SWFC.Application.M800_Security.M806_AccessControl.Roles;

public sealed record RoleDetailsDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    bool IsSystemRole);

public sealed record RoleListItem(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    bool IsSystemRole);
