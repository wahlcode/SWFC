using SWFC.Application.M100_System.M102_Organization.Interfaces;

namespace SWFC.Application.M100_System.M102_Organization.Services;

public sealed class M102SecurityProjectionService : IM102SecurityProjectionService
{
    private readonly IUserReadRepository _userReadRepository;
    private readonly IRoleReadRepository _roleReadRepository;
    private readonly IRolePermissionMapper _rolePermissionMapper;

    public M102SecurityProjectionService(
        IUserReadRepository userReadRepository,
        IRoleReadRepository roleReadRepository,
        IRolePermissionMapper rolePermissionMapper)
    {
        _userReadRepository = userReadRepository;
        _roleReadRepository = roleReadRepository;
        _rolePermissionMapper = rolePermissionMapper;
    }

    public async Task<M102SecurityProjection?> GetByIdentityKeyAsync(
        string identityKey,
        CancellationToken cancellationToken = default)
    {
        var user = await _userReadRepository.GetByIdentityKeyAsync(identityKey, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var roleIds = user.Roles
            .Select(x => x.RoleId)
            .Distinct()
            .ToArray();

        var resolvedRoles = new List<string>();

        foreach (var roleId in roleIds)
        {
            var role = await _roleReadRepository.GetByIdAsync(roleId, cancellationToken);

            if (role is not null)
            {
                resolvedRoles.Add(role.Name.Value);
            }
        }

        var permissions = _rolePermissionMapper.Map(resolvedRoles);

        return new M102SecurityProjection(
            user.Id,
            user.IdentityKey.Value,
            user.DisplayName.Value,
            user.IsActive,
            resolvedRoles,
            permissions);
    }
}