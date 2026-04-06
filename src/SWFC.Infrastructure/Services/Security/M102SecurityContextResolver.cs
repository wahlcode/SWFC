using SWFC.Application.M100_System.M102_Organization.Interfaces;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;

namespace SWFC.Infrastructure.Services.Security;

public sealed class M102SecurityContextResolver
{
    private readonly IM102SecurityProjectionService _projectionService;

    public M102SecurityContextResolver(IM102SecurityProjectionService projectionService)
    {
        _projectionService = projectionService;
    }

    public async Task<SecurityContext> ResolveAsync(
        string identityKey,
        string fallbackUsername,
        bool isAuthenticated,
        CancellationToken cancellationToken = default)
    {
        if (!isAuthenticated || string.IsNullOrWhiteSpace(identityKey))
        {
            return new SecurityContext(
                userId: string.Empty,
                identityKey: string.Empty,
                username: string.Empty,
                displayName: string.Empty,
                isAuthenticated: false,
                roles: Array.Empty<string>(),
                permissions: Array.Empty<string>());
        }

        var normalizedIdentityKey = identityKey.Trim();
        var normalizedFallbackUsername = string.IsNullOrWhiteSpace(fallbackUsername)
            ? normalizedIdentityKey
            : fallbackUsername.Trim();

        var projection = await _projectionService.GetByIdentityKeyAsync(
            normalizedIdentityKey,
            cancellationToken);

        if (projection is null || !projection.IsActive)
        {
            return new SecurityContext(
                userId: string.Empty,
                identityKey: normalizedIdentityKey,
                username: normalizedFallbackUsername,
                displayName: normalizedFallbackUsername,
                isAuthenticated: true,
                roles: Array.Empty<string>(),
                permissions: Array.Empty<string>());
        }

        return new SecurityContext(
            userId: projection.UserId.ToString(),
            identityKey: projection.IdentityKey,
            username: projection.Username,
            displayName: projection.DisplayName,
            isAuthenticated: true,
            roles: projection.Roles,
            permissions: projection.Permissions);
    }
}