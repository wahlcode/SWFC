namespace SWFC.Application.M800_Security.M802_ApplicationSecurity;

public sealed class SecurityContext
{
    public SecurityContext(
        string userId,
        string username,
        bool isAuthenticated,
        IReadOnlyCollection<string>? roles = null,
        IReadOnlyCollection<string>? permissions = null)
    {
        UserId = userId;
        Username = username;
        IsAuthenticated = isAuthenticated;
        Roles = roles ?? Array.Empty<string>();
        Permissions = permissions ?? Array.Empty<string>();
    }

    public string UserId { get; }
    public string Username { get; }
    public bool IsAuthenticated { get; }
    public IReadOnlyCollection<string> Roles { get; }
    public IReadOnlyCollection<string> Permissions { get; }

    public bool HasRole(string role) =>
        Roles.Any(x => string.Equals(x, role, StringComparison.OrdinalIgnoreCase));

    public bool HasPermission(string permission) =>
        Permissions.Any(x => string.Equals(x, permission, StringComparison.OrdinalIgnoreCase));
}