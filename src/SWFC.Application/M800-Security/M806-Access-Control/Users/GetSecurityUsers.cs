using SWFC.Application.M100_System.M102_Organization.Users;
using SWFC.Application.M100_System.M103_Authentication;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M806_AccessControl.Shared;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M800_Security.M806_AccessControl.Users;

public sealed record GetSecurityUsersQuery;

public sealed class GetSecurityUsersPolicy : IAuthorizationPolicy<GetSecurityUsersQuery>
{
    public AuthorizationRequirement GetRequirement(GetSecurityUsersQuery request) =>
        new(requiredPermissions: new[] { "security.read" });
}

public sealed class GetSecurityUsersHandler : IUseCaseHandler<GetSecurityUsersQuery, IReadOnlyList<SecurityUserListItemDto>>
{
    private readonly IUserReadRepository _userReadRepository;
    private readonly ISecurityAdministrationGuard _securityAdministrationGuard;
    private readonly ILocalAuthenticationService _localAuthenticationService;

    public GetSecurityUsersHandler(
        IUserReadRepository userReadRepository,
        ISecurityAdministrationGuard securityAdministrationGuard,
        ILocalAuthenticationService localAuthenticationService)
    {
        _userReadRepository = userReadRepository;
        _securityAdministrationGuard = securityAdministrationGuard;
        _localAuthenticationService = localAuthenticationService;
    }

    public async Task<Result<IReadOnlyList<SecurityUserListItemDto>>> HandleAsync(
        GetSecurityUsersQuery command,
        CancellationToken cancellationToken = default)
    {
        var users = await _userReadRepository.GetAllAsync(cancellationToken);
        var items = new List<SecurityUserListItemDto>(users.Count);

        foreach (var user in users
                     .OrderBy(x => x.DisplayName.Value, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(x => x.Username.Value, StringComparer.OrdinalIgnoreCase))
        {
            var roleNames = await _securityAdministrationGuard.GetActiveRoleNamesAsync(user.Id, cancellationToken);
            var credentialStatus = await _localAuthenticationService.GetStatusAsync(user.Id, cancellationToken);
            var isProtectedSuperAdmin = await _securityAdministrationGuard.IsProtectedSuperAdminAsync(user.Id, cancellationToken);

            items.Add(new SecurityUserListItemDto(
                user.Id,
                user.Username.Value,
                user.DisplayName.Value,
                user.BusinessEmail,
                user.Status,
                user.UserType,
                roleNames.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToArray(),
                isProtectedSuperAdmin,
                credentialStatus));
        }

        return Result<IReadOnlyList<SecurityUserListItemDto>>.Success(items);
    }
}
