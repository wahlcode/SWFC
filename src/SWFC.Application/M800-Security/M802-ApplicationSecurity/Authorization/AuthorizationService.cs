using SWFC.Application.M800_Security.M802_ApplicationSecurity;

namespace SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;

public sealed class AuthorizationService : IAuthorizationService
{
    public Task<AuthorizationResult> AuthorizeAsync(
        SecurityContext securityContext,
        AuthorizationRequirement requirement,
        CancellationToken cancellationToken = default)
    {
        if (!securityContext.IsAuthenticated)
        {
            return Task.FromResult(AuthorizationResult.Forbidden("User is not authenticated."));
        }

        foreach (var role in requirement.RequiredRoles)
        {
            if (!securityContext.HasRole(role))
            {
                return Task.FromResult(
                    AuthorizationResult.Forbidden($"Missing required role: {role}."));
            }
        }

        foreach (var permission in requirement.RequiredPermissions)
        {
            if (!securityContext.HasPermission(permission))
            {
                return Task.FromResult(
                    AuthorizationResult.Forbidden($"Missing required permission: {permission}."));
            }
        }

        return Task.FromResult(AuthorizationResult.Authorized());
    }
}