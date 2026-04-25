namespace SWFC.Application.M800_Security.M806_AccessControl.Shared;

public interface IM102SecurityProjectionService
{
    Task<M102SecurityProjection?> GetByIdentityKeyAsync(
        string identityKey,
        CancellationToken cancellationToken = default);
}

public sealed record M102SecurityProjection(
    Guid UserId,
    string IdentityKey,
    string Username,
    string DisplayName,
    string PreferredCultureName,
    bool IsActive,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions,
    IReadOnlyCollection<string> PermissionModules);