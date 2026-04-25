namespace SWFC.Application.M800_Security.M806_AccessControl.RolePermissions;

public interface IRolePermissionReadRepository
{
    Task<IReadOnlyList<RolePermissionListItemDto>> GetByRoleIdAsync(
        Guid roleId,
        CancellationToken cancellationToken = default);
}

public interface IRolePermissionWriteRepository
{
    Task SetPermissionsAsync(
        Guid roleId,
        IReadOnlyCollection<Guid> permissionIds,
        string changedByUserId,
        CancellationToken cancellationToken = default);
}
