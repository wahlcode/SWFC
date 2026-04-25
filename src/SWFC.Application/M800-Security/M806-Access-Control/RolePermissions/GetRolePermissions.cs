using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M800_Security.M806_AccessControl.RolePermissions;

public sealed record GetRolePermissionsQuery(Guid RoleId);

public sealed class GetRolePermissionsValidator : ICommandValidator<GetRolePermissionsQuery>
{
    public Task<ValidationResult> ValidateAsync(
        GetRolePermissionsQuery command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.RoleId == Guid.Empty)
        {
            result.Add("RoleId", "Role id is required.");
        }

        return Task.FromResult(result);
    }
}

public sealed class GetRolePermissionsPolicy : IAuthorizationPolicy<GetRolePermissionsQuery>
{
    public AuthorizationRequirement GetRequirement(GetRolePermissionsQuery request) =>
        new(requiredPermissions: new[] { "security.read" });
}

public sealed class GetRolePermissionsHandler : IUseCaseHandler<GetRolePermissionsQuery, IReadOnlyList<RolePermissionListItemDto>>
{
    private readonly IRolePermissionReadRepository _rolePermissionReadRepository;

    public GetRolePermissionsHandler(IRolePermissionReadRepository rolePermissionReadRepository)
    {
        _rolePermissionReadRepository = rolePermissionReadRepository;
    }

    public async Task<Result<IReadOnlyList<RolePermissionListItemDto>>> HandleAsync(
        GetRolePermissionsQuery command,
        CancellationToken cancellationToken = default)
    {
        var items = await _rolePermissionReadRepository.GetByRoleIdAsync(command.RoleId, cancellationToken);
        return Result<IReadOnlyList<RolePermissionListItemDto>>.Success(items);
    }
}
