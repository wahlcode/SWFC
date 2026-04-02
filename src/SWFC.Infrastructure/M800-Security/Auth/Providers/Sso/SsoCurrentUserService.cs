using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;

namespace SWFC.Infrastructure.M800_Security.Auth.Providers.Sso;

public sealed class SsoCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SsoCurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<SecurityContext> GetSecurityContextAsync(CancellationToken cancellationToken = default)
    {
        var principal = _httpContextAccessor.HttpContext?.User;

        if (principal?.Identity?.IsAuthenticated != true)
        {
            return Task.FromResult(
                new SecurityContext(
                    userId: string.Empty,
                    username: string.Empty,
                    isAuthenticated: false,
                    roles: Array.Empty<string>(),
                    permissions: Array.Empty<string>()));
        }

        var userId =
            principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub")
            ?? principal.Identity?.Name
            ?? string.Empty;

        var username =
            principal.Identity?.Name
            ?? principal.FindFirstValue(ClaimTypes.Name)
            ?? userId;

        var roles = principal.FindAll(ClaimTypes.Role)
            .Select(x => x.Value)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var permissions = principal.FindAll("permission")
            .Select(x => x.Value)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var securityContext = new SecurityContext(
            userId: userId,
            username: username,
            isAuthenticated: true,
            roles: roles,
            permissions: permissions);

        return Task.FromResult(securityContext);
    }
}