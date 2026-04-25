namespace SWFC.Application.M800_Security.M806_AccessControl.Permissions;

public sealed record PermissionListItemDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    string Module,
    bool IsActive);
