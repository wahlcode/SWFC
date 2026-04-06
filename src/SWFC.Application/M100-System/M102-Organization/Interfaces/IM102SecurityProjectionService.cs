namespace SWFC.Application.M100_System.M102_Organization.Interfaces;

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
    bool IsActive,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions);