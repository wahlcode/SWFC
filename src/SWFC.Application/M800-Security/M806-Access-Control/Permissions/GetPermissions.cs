using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M800_Security.M806_AccessControl.Permissions;

public sealed record GetPermissionsQuery;

public sealed class GetPermissionsValidator : ICommandValidator<GetPermissionsQuery>
{
    public Task<ValidationResult> ValidateAsync(
        GetPermissionsQuery command,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ValidationResult.Success());
    }
}

public sealed class GetPermissionsPolicy : IAuthorizationPolicy<GetPermissionsQuery>
{
    public AuthorizationRequirement GetRequirement(GetPermissionsQuery request) =>
        new(requiredPermissions: new[] { "security.read" });
}

public sealed class GetPermissionsHandler : IUseCaseHandler<GetPermissionsQuery, IReadOnlyList<PermissionListItemDto>>
{
    private readonly IPermissionReadRepository _permissionReadRepository;

    public GetPermissionsHandler(IPermissionReadRepository permissionReadRepository)
    {
        _permissionReadRepository = permissionReadRepository;
    }

    public async Task<Result<IReadOnlyList<PermissionListItemDto>>> HandleAsync(
        GetPermissionsQuery command,
        CancellationToken cancellationToken = default)
    {
        var permissions = await _permissionReadRepository.GetAllAsync(cancellationToken);
        return Result<IReadOnlyList<PermissionListItemDto>>.Success(permissions);
    }
}
