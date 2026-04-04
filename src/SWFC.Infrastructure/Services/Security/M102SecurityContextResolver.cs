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
                username: string.Empty,
                isAuthenticated: false,
                roles: Array.Empty<string>(),
                permissions: Array.Empty<string>());
        }

        var projection = await _projectionService.GetByIdentityKeyAsync(identityKey, cancellationToken);

        if (projection is null || !projection.IsActive)
        {
            return new SecurityContext(
                userId: identityKey,
                username: fallbackUsername,
                isAuthenticated: true,
                roles: Array.Empty<string>(),
                permissions: Array.Empty<string>());
        }

        return new SecurityContext(
            userId: projection.UserId.ToString(),
            username: projection.DisplayName,
            isAuthenticated: true,
            roles: projection.Roles,
            permissions: projection.Permissions);
    }
}