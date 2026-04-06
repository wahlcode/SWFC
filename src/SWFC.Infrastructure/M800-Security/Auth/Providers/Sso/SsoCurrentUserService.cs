using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Infrastructure.Services.Security;

namespace SWFC.Infrastructure.M800_Security.Auth.Providers.Sso;

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
            return Task.FromResult(
                new SecurityContext(
                    userId: string.Empty,
                    identityKey: string.Empty,
                    username: string.Empty,
                    displayName: string.Empty,
                    isAuthenticated: false,
                    roles: Array.Empty<string>(),
                    permissions: Array.Empty<string>()));
        }

        var identityKey =
            principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub")
            ?? principal.Identity?.Name
            ?? string.Empty;

        var fallbackUsername =
            principal.Identity?.Name
            ?? principal.FindFirstValue(ClaimTypes.Name)
            ?? identityKey;

        return _securityContextResolver.ResolveAsync(
            identityKey: identityKey,
            fallbackUsername: fallbackUsername,
            isAuthenticated: true,
            cancellationToken: cancellationToken);
    }
}