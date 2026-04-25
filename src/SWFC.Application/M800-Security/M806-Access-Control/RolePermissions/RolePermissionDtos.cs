namespace SWFC.Application.M800_Security.M806_AccessControl.RolePermissions;

public sealed record RolePermissionListItemDto(
    Guid PermissionId,
    string PermissionCode,
    string PermissionName,
    string? PermissionDescription,
    string Module,
    bool IsAssigned,
    bool IsPermissionActive);

public sealed record SetRolePermissionsCommand(
    Guid RoleId,
    IReadOnlyCollection<Guid> PermissionIds,
    string Reason);
