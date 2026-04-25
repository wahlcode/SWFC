using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Infrastructure.Services.Security;

namespace SWFC.Infrastructure.M100_System.M103_Authentication.Providers.Sso;

public sealed class SsoCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly M102SecurityContextResolver _securityContextResolver;

    public SsoCurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        M102SecurityContextResolver securityContextResolver)
    {
        _httpContextAccessor = httpContextAccessor;
        _securityContextResolver = securityContextResolver;
    }

    public Task<SecurityContext> GetSecurityContextAsync(CancellationToken cancellationToken = default)
    {
        var principal = _httpContextAccessor.HttpContext?.User;

        if (principal?.Identity?.IsAuthenticated != true)
        {
            return _securityContextResolver.ResolveAsync(
                userId: string.Empty,
                identityKey: string.Empty,
                fallbackUsername: string.Empty,
                isAuthenticated: false,
                isDeveloperMode: false,
                cancellationToken: cancellationToken);
        }

        var userId =
            principal.FindFirstValue(ClaimTypes.NameIdentifier) ??
            principal.FindFirstValue("sub") ??
            string.Empty;

        var identityKey =
            principal.FindFirstValue(SecurityClaimTypes.IdentityKey) ??
            principal.FindFirstValue(ClaimTypes.Email) ??
            principal.FindFirstValue(ClaimTypes.Upn) ??
            principal.FindFirstValue(ClaimTypes.NameIdentifier) ??
            string.Empty;

        var fallbackUsername =
            principal.FindFirstValue(ClaimTypes.Name) ??
            principal.Identity?.Name ??
            identityKey;

        return _securityContextResolver.ResolveAsync(
            userId: userId,
            identityKey: identityKey,
            fallbackUsername: fallbackUsername,
            isAuthenticated: true,
            isDeveloperMode: false,
            cancellationToken: cancellationToken);
    }
}

