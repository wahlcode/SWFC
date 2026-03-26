namespace SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;

public sealed class AuthorizationRequirement
{
    public AuthorizationRequirement(
        IReadOnlyCollection<string>? requiredRoles = null,
        IReadOnlyCollection<string>? requiredPermissions = null)
    {
        RequiredRoles = requiredRoles ?? Array.Empty<string>();
        RequiredPermissions = requiredPermissions ?? Array.Empty<string>();
    }

    public IReadOnlyCollection<string> RequiredRoles { get; }
    public IReadOnlyCollection<string> RequiredPermissions { get; }

    public static AuthorizationRequirement None() => new();
}