using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M806_AccessControl.Decisions;

namespace SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;

public sealed class AuthorizationService : IAuthorizationService
{
    private readonly IAccessDecisionService _accessDecisionService;

    public AuthorizationService()
        : this(new AccessDecisionService())
    {
    }

    public AuthorizationService(IAccessDecisionService accessDecisionService)
    {
        _accessDecisionService = accessDecisionService;
    }

    public Task<AuthorizationResult> AuthorizeAsync(
        SecurityContext securityContext,
        AuthorizationRequirement requirement,
        CancellationToken cancellationToken = default)
    {
        return AuthorizeCoreAsync(securityContext, requirement, cancellationToken);
    }

    private async Task<AuthorizationResult> AuthorizeCoreAsync(
        SecurityContext securityContext,
        AuthorizationRequirement requirement,
        CancellationToken cancellationToken)
    {
        var decision = await _accessDecisionService.DecideAsync(
            securityContext,
            AccessDecisionRequest.ForAuthorizationRequirement(
                "System",
                requirement.RequiredRoles,
                requirement.RequiredPermissions),
            cancellationToken);

        return decision.IsAllowed
            ? AuthorizationResult.Authorized()
            : AuthorizationResult.Forbidden(decision.Reason);
    }
}

