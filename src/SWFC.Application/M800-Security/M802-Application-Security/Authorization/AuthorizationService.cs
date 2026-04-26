using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Application.M800_Security.M806_AccessControl.Decisions;

namespace SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;

public sealed class AuthorizationService : IAuthorizationService
{
    private readonly IAccessDecisionService _accessDecisionService;
    private readonly IAuditService? _auditService;

    public AuthorizationService()
        : this(new AccessDecisionService())
    {
    }

    public AuthorizationService(IAccessDecisionService accessDecisionService)
        : this(accessDecisionService, null)
    {
    }

    public AuthorizationService(
        IAccessDecisionService accessDecisionService,
        IAuditService? auditService)
    {
        _accessDecisionService = accessDecisionService;
        _auditService = auditService;
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

        if (_auditService is not null &&
            securityContext.IsAuthenticated &&
            (requirement.RequiredRoles.Count > 0 ||
             requirement.RequiredPermissions.Count > 0 ||
             !decision.IsAllowed))
        {
            await _auditService.WriteAsync(
                decision.ToAuditWriteRequest(securityContext, DateTime.UtcNow),
                cancellationToken);
        }

        return decision.IsAllowed
            ? AuthorizationResult.Authorized()
            : AuthorizationResult.Forbidden(decision.Reason);
    }
}

