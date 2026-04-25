using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Infrastructure.M400_Integration.M406_IdentityIntegration.Services;
using SWFC.Infrastructure.Services.Security;

namespace SWFC.Infrastructure.M100_System.M103_Authentication.Services;

public sealed class OidcAuthenticationFlowService
{
    private readonly OidcExternalIdentityResolver _externalIdentityResolver;
    private readonly M102SecurityContextResolver _securityContextResolver;
    private readonly ClaimsPrincipalFactory _claimsPrincipalFactory;
    private readonly IAuditService _auditService;

    public OidcAuthenticationFlowService(
        OidcExternalIdentityResolver externalIdentityResolver,
        M102SecurityContextResolver securityContextResolver,
        ClaimsPrincipalFactory claimsPrincipalFactory,
        IAuditService auditService)
    {
        _externalIdentityResolver = externalIdentityResolver;
        _securityContextResolver = securityContextResolver;
        _claimsPrincipalFactory = claimsPrincipalFactory;
        _auditService = auditService;
    }

    public async Task<ClaimsPrincipal> CreateInternalPrincipalAsync(
        ClaimsPrincipal validatedPrincipal,
        CancellationToken cancellationToken = default)
    {
        var externalIdentity = _externalIdentityResolver.Resolve(validatedPrincipal);

        var securityContext = await _securityContextResolver.ResolveAsync(
            userId: string.Empty,
            identityKey: externalIdentity.IdentityKey,
            fallbackUsername: externalIdentity.DisplayName,
            isAuthenticated: true,
            isDeveloperMode: false,
            cancellationToken: cancellationToken);

        if (!securityContext.IsAuthenticated ||
            !Guid.TryParse(securityContext.UserId, out var userId) ||
            userId == Guid.Empty)
        {
            await WriteAuditAsync(
                action: "SsoCallbackFailed",
                actorUserId: "anonymous",
                actorDisplayName: externalIdentity.DisplayName,
                objectType: "OidcIdentity",
                objectId: externalIdentity.Subject,
                targetUserId: null,
                reason: "No matching SWFC user found for the validated external identity.",
                cancellationToken);

            throw new InvalidOperationException("No matching SWFC user found for the validated external identity.");
        }

        await WriteAuditAsync(
            action: "LoginSuccess",
            actorUserId: securityContext.UserId,
            actorDisplayName: securityContext.DisplayName,
            objectType: "Authentication",
            objectId: securityContext.UserId,
            targetUserId: securityContext.UserId,
            reason: "OIDC authentication succeeded.",
            cancellationToken);

        await WriteAuditAsync(
            action: "SessionCreated",
            actorUserId: securityContext.UserId,
            actorDisplayName: securityContext.DisplayName,
            objectType: "AuthenticationSession",
            objectId: securityContext.UserId,
            targetUserId: securityContext.UserId,
            reason: "Internal SWFC session created after external authentication.",
            cancellationToken);

        var principal = _claimsPrincipalFactory.Create(
            securityContext,
            CookieAuthenticationDefaults.AuthenticationScheme);

        if (principal.Identity is ClaimsIdentity identity)
        {
            foreach (var authenticationMethod in externalIdentity.AuthenticationMethods)
            {
                identity.AddClaim(new Claim(SecurityClaimTypes.AuthenticationMethod, authenticationMethod));
            }

            if (!string.IsNullOrWhiteSpace(externalIdentity.AuthenticationContext))
            {
                identity.AddClaim(new Claim(
                    SecurityClaimTypes.AuthenticationContext,
                    externalIdentity.AuthenticationContext));
            }
        }

        return principal;
    }

    public Task WriteRemoteFailureAsync(
        string actorDisplayName,
        string objectId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        return WriteAuditAsync(
            action: "SsoCallbackFailed",
            actorUserId: "anonymous",
            actorDisplayName: actorDisplayName,
            objectType: "OidcIdentity",
            objectId: objectId,
            targetUserId: null,
            reason: reason,
            cancellationToken);
    }

    public Task WriteTokenFailureAsync(
        string actorDisplayName,
        string objectId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        return WriteAuditAsync(
            action: "InvalidExternalToken",
            actorUserId: "anonymous",
            actorDisplayName: actorDisplayName,
            objectType: "OidcIdentity",
            objectId: objectId,
            targetUserId: null,
            reason: reason,
            cancellationToken);
    }

    private Task WriteAuditAsync(
        string action,
        string actorUserId,
        string actorDisplayName,
        string objectType,
        string objectId,
        string? targetUserId,
        string reason,
        CancellationToken cancellationToken)
    {
        return _auditService.WriteAsync(
            new AuditWriteRequest(
                ActorUserId: actorUserId,
                ActorDisplayName: actorDisplayName,
                Action: action,
                Module: "M103",
                ObjectType: objectType,
                ObjectId: objectId,
                TimestampUtc: DateTime.UtcNow,
                TargetUserId: targetUserId,
                Reason: reason),
            cancellationToken);
    }
}
