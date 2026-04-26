namespace SWFC.Application.M800_Security.M802_ApplicationSecurity;

public sealed class SecurityContext
{
    public SecurityContext(
        string userId,
        string identityKey,
        string username,
        string displayName,
        bool isAuthenticated,
        bool isDeveloperMode = false,
        bool canUseDeveloperMode = false,
        IReadOnlyCollection<string>? roles = null,
        IReadOnlyCollection<string>? permissions = null,
        IReadOnlyCollection<string>? permissionModules = null,
        IReadOnlyCollection<string>? organizationUnitIds = null,
        string preferredCultureName = "en-US")
    {
        UserId = userId;
        IdentityKey = identityKey;
        Username = username;
        DisplayName = displayName;
        IsAuthenticated = isAuthenticated;
        IsDeveloperMode = isDeveloperMode;
        CanUseDeveloperMode = canUseDeveloperMode;
        Roles = roles ?? Array.Empty<string>();
        Permissions = permissions ?? Array.Empty<string>();
        PermissionModules = permissionModules ?? Array.Empty<string>();
        OrganizationUnitIds = organizationUnitIds ?? Array.Empty<string>();
        PreferredCultureName = NormalizeCultureName(preferredCultureName);
    }

    public string UserId { get; }
    public string IdentityKey { get; }
    public string Username { get; }
    public string DisplayName { get; }
    public bool IsAuthenticated { get; }
    public bool IsDeveloperMode { get; }
    public bool CanUseDeveloperMode { get; }
    public IReadOnlyCollection<string> Roles { get; }
    public IReadOnlyCollection<string> Permissions { get; }
    public IReadOnlyCollection<string> PermissionModules { get; }
    public IReadOnlyCollection<string> OrganizationUnitIds { get; }
    public string PreferredCultureName { get; }

    public bool HasRole(string role) =>
        Roles.Any(x => string.Equals(x, role, StringComparison.OrdinalIgnoreCase));

    public bool HasPermission(string permission) =>
        IsDeveloperMode ||
        Permissions.Any(x =>
            string.Equals(x, permission, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(x, "*", StringComparison.OrdinalIgnoreCase) ||
            MatchesWildcardPermission(x, permission));

    public bool HasModuleAccess(string moduleCode) =>
        IsDeveloperMode ||
        PermissionModules.Any(x =>
            string.Equals(x, moduleCode, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(x, "*", StringComparison.OrdinalIgnoreCase));

    private static bool MatchesWildcardPermission(string assignedPermission, string requestedPermission)
    {
        if (!assignedPermission.EndsWith(".*", StringComparison.Ordinal))
        {
            return false;
        }

        var prefix = assignedPermission[..^1];

        return requestedPermission.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeCultureName(string? cultureName)
    {
        return string.IsNullOrWhiteSpace(cultureName)
            ? "de-DE"
            : cultureName.Trim();
    }
}
