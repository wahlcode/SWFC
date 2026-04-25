using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Infrastructure.Services.Security;

namespace SWFC.Infrastructure.M100_System.M103_Authentication.Providers.Local;

public sealed class LocalCurrentUserService : ICurrentUserService
{
    private static readonly SemaphoreSlim ResolveLock = new(1, 1);

    private const string SecurityContextCacheKey = "__SWFC_SECURITY_CONTEXT__";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly M102SecurityContextResolver _securityContextResolver;

    public LocalCurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        M102SecurityContextResolver securityContextResolver)
    {
        _httpContextAccessor = httpContextAccessor;
        _securityContextResolver = securityContextResolver;
    }

    public async Task<SecurityContext> GetSecurityContextAsync(CancellationToken cancellationToken = default)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is not null &&
            httpContext.Items.TryGetValue(SecurityContextCacheKey, out var cachedValue) &&
            cachedValue is SecurityContext cachedContext)
        {
            return cachedContext;
        }

        await ResolveLock.WaitAsync(cancellationToken);

        try
        {
            if (httpContext is not null &&
                httpContext.Items.TryGetValue(SecurityContextCacheKey, out cachedValue) &&
                cachedValue is SecurityContext cachedAfterLock)
            {
                return cachedAfterLock;
            }

            var principal = httpContext?.User;

            SecurityContext resolvedContext;

            if (principal?.Identity?.IsAuthenticated != true)
            {
                resolvedContext = await _securityContextResolver.ResolveAsync(
                    userId: string.Empty,
                    identityKey: string.Empty,
                    fallbackUsername: string.Empty,
                    isAuthenticated: false,
                    isDeveloperMode: false,
                    cancellationToken: cancellationToken);
            }
            else
            {
                var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
                var identityKey = principal.FindFirstValue(SecurityClaimTypes.IdentityKey) ?? string.Empty;
                var fallbackUsername = principal.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
                var isDeveloperMode = string.Equals(
                    principal.FindFirstValue(SecurityClaimTypes.DeveloperMode),
                    "true",
                    StringComparison.OrdinalIgnoreCase);

                resolvedContext = await _securityContextResolver.ResolveAsync(
                    userId: userId,
                    identityKey: identityKey,
                    fallbackUsername: fallbackUsername,
                    isAuthenticated: true,
                    isDeveloperMode: isDeveloperMode,
                    cancellationToken: cancellationToken);
            }

            if (httpContext is not null)
            {
                httpContext.Items[SecurityContextCacheKey] = resolvedContext;
            }

            return resolvedContext;
        }
        finally
        {
            ResolveLock.Release();
        }
    }
}
