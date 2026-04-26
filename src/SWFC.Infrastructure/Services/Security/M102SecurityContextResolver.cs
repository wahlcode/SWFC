using Microsoft.Extensions.Options;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M806_AccessControl.Shared;
using SWFC.Infrastructure.M100_System.M103_Authentication.Configuration;
using SWFC.Infrastructure.M100_System.M107_SetupDeployment;

namespace SWFC.Infrastructure.Services.Security;

public sealed class M102SecurityContextResolver
{
    private const string DefaultPreferredCultureName = "en-US";

    private readonly IM102SecurityProjectionService _projectionService;
    private readonly AuthenticationOptions _authenticationOptions;
    private readonly M107SetupOptions _initializationOptions;

    public M102SecurityContextResolver(
        IM102SecurityProjectionService projectionService,
        IOptions<AuthenticationOptions> authenticationOptions,
        IOptions<M107SetupOptions> initializationOptions)
    {
        _projectionService = projectionService;
        _authenticationOptions = authenticationOptions.Value;
        _initializationOptions = initializationOptions.Value;
    }

    public async Task<SecurityContext> ResolveAsync(
        string userId,
        string identityKey,
        string fallbackUsername,
        bool isAuthenticated,
        bool isDeveloperMode,
        CancellationToken cancellationToken = default)
    {
        var normalizedUserId = string.IsNullOrWhiteSpace(userId)
            ? string.Empty
            : userId.Trim();

        if (!isAuthenticated || string.IsNullOrWhiteSpace(identityKey))
        {
            return new SecurityContext(
                userId: string.Empty,
                identityKey: string.Empty,
                username: string.Empty,
                displayName: string.Empty,
                isAuthenticated: false,
                isDeveloperMode: false,
                canUseDeveloperMode: false,
                roles: Array.Empty<string>(),
                permissions: Array.Empty<string>(),
                permissionModules: Array.Empty<string>(),
                preferredCultureName: DefaultPreferredCultureName);
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
                userId: normalizedUserId,
                identityKey: normalizedIdentityKey,
                username: normalizedFallbackUsername,
                displayName: normalizedFallbackUsername,
                isAuthenticated: true,
                isDeveloperMode: false,
                canUseDeveloperMode: false,
                roles: Array.Empty<string>(),
                permissions: Array.Empty<string>(),
                permissionModules: Array.Empty<string>(),
                preferredCultureName: DefaultPreferredCultureName);
        }

        var canUseDeveloperMode =
            projection.Roles.Any(x => string.Equals(x, _initializationOptions.SuperAdminRoleName, StringComparison.OrdinalIgnoreCase)) &&
            (string.Equals(
                projection.Username,
                _authenticationOptions.InitialSuperAdmin.Username,
                StringComparison.OrdinalIgnoreCase) ||
             string.Equals(
                projection.IdentityKey,
                _initializationOptions.SuperAdminIdentityKey,
                StringComparison.OrdinalIgnoreCase));

        return new SecurityContext(
            userId: projection.UserId.ToString(),
            identityKey: projection.IdentityKey,
            username: projection.Username,
            displayName: projection.DisplayName,
            isAuthenticated: true,
            isDeveloperMode: canUseDeveloperMode && isDeveloperMode,
            canUseDeveloperMode: canUseDeveloperMode,
            roles: projection.Roles,
            permissions: projection.Permissions,
            permissionModules: projection.PermissionModules,
            preferredCultureName: projection.PreferredCultureName);
    }
}
