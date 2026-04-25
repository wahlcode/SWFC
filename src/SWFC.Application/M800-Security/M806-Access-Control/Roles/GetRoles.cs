using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M800_Security.M806_AccessControl.Roles;

public sealed record GetRolesQuery;

public sealed class GetRolesPolicy : IAuthorizationPolicy<GetRolesQuery>
{
    public AuthorizationRequirement GetRequirement(GetRolesQuery request) =>
        new(requiredPermissions: new[] { "security.read" });
}

public sealed class GetRolesHandler : IUseCaseHandler<GetRolesQuery, IReadOnlyList<RoleListItem>>
{
    private readonly IRoleReadRepository _roleReadRepository;

    public GetRolesHandler(IRoleReadRepository roleReadRepository)
    {
        _roleReadRepository = roleReadRepository;
    }

    public async Task<Result<IReadOnlyList<RoleListItem>>> HandleAsync(
        GetRolesQuery command,
        CancellationToken cancellationToken = default)
    {
        var roles = await _roleReadRepository.GetAllAsync(cancellationToken);

        var items = roles
            .Select(x => new RoleListItem(
                x.Id,
                x.Name.Value,
                x.Description,
                x.IsActive,
                x.IsSystemRole))
            .ToList();

        return Result<IReadOnlyList<RoleListItem>>.Success(items);
    }
}
