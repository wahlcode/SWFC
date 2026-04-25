using SWFC.Application.M100_System.M102_Organization.Users;
using SWFC.Application.M800_Security.M806_AccessControl.Assignments;
using SWFC.Application.M800_Security.M806_AccessControl.RolePermissions;
using SWFC.Application.M800_Security.M806_AccessControl.Roles;

namespace SWFC.Application.M800_Security.M806_AccessControl.Shared;

public sealed class M102SecurityProjectionService : IM102SecurityProjectionService
{
    private readonly IUserReadRepository _userReadRepository;
    private readonly IUserRoleReadRepository _userRoleReadRepository;
    private readonly IRoleReadRepository _roleReadRepository;
    private readonly IRolePermissionReadRepository _rolePermissionReadRepository;

    public M102SecurityProjectionService(
        IUserReadRepository userReadRepository,
        IUserRoleReadRepository userRoleReadRepository,
        IRoleReadRepository roleReadRepository,
        IRolePermissionReadRepository rolePermissionReadRepository)
    {
        _userReadRepository = userReadRepository;
        _userRoleReadRepository = userRoleReadRepository;
        _roleReadRepository = roleReadRepository;
        _rolePermissionReadRepository = rolePermissionReadRepository;
    }

    public async Task<M102SecurityProjection?> GetByIdentityKeyAsync(
        string identityKey,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(identityKey))
        {
            return null;
        }

        var user = await _userReadRepository.GetByIdentityKeyAsync(identityKey, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var roleIds = await _userRoleReadRepository.GetActiveRoleIdsByUserIdAsync(
            user.Id,
            cancellationToken);

        var resolvedRoles = new List<string>();
        var resolvedPermissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var resolvedPermissionModules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var roleId in roleIds)
        {
            var role = await _roleReadRepository.GetByIdAsync(roleId, cancellationToken);

            if (role is null)
            {
                continue;
            }

            resolvedRoles.Add(role.Name.Value);

            var rolePermissions = await _rolePermissionReadRepository.GetByRoleIdAsync(
                roleId,
                cancellationToken);

            foreach (var permission in rolePermissions)
            {
                if (permission.IsAssigned && permission.IsPermissionActive)
                {
                    resolvedPermissions.Add(permission.PermissionCode);
                    resolvedPermissionModules.Add(permission.Module);
                }
            }
        }

        return new M102SecurityProjection(
            user.Id,
            user.IdentityKey.Value,
            user.Username.Value,
            user.DisplayName.Value,
            user.PreferredCultureName,
            user.IsActive,
            resolvedRoles,
            resolvedPermissions.ToArray(),
            resolvedPermissionModules.ToArray());
    }
}