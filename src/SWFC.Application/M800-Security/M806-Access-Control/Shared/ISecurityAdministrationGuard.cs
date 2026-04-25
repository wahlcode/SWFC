using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M800_Security.M806_AccessControl.Shared;

public interface ISecurityAdministrationGuard
{
    Task<Error?> ValidateRoleAssignmentAsync(
        SecurityContext actor,
        Guid targetUserId,
        string targetRoleName,
        CancellationToken cancellationToken = default);

    Task<Error?> ValidateRoleRemovalAsync(
        SecurityContext actor,
        Guid targetUserId,
        string targetRoleName,
        CancellationToken cancellationToken = default);

    Task<Error?> ValidateAdminPasswordResetAsync(
        SecurityContext actor,
        Guid targetUserId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetActiveRoleNamesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<bool> IsProtectedSuperAdminAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
