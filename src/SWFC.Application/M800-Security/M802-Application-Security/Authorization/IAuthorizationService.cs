using SWFC.Application.M800_Security.M802_ApplicationSecurity;

namespace SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;

public interface IAuthorizationService
{
    Task<AuthorizationResult> AuthorizeAsync(
        SecurityContext securityContext,
        AuthorizationRequirement requirement,
        CancellationToken cancellationToken = default);
}

