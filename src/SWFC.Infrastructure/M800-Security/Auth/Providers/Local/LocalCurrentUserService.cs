using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Infrastructure.Services.Security;

namespace SWFC.Infrastructure.M800_Security.Auth.Providers.Local;

public sealed class LocalCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly M102SecurityContextResolver _securityContextResolver;

    public LocalCurrentUserService(
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
                identityKey: string.Empty,
                fallbackUsername: string.Empty,
                isAuthenticated: false,
                cancellationToken: cancellationToken);
        }

        var identityKey = principal.FindFirstValue("identity_key") ?? string.Empty;
        var fallbackUsername = principal.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

        return _securityContextResolver.ResolveAsync(
            identityKey: identityKey,
            fallbackUsername: fallbackUsername,
            isAuthenticated: true,
            cancellationToken: cancellationToken);
    }
}