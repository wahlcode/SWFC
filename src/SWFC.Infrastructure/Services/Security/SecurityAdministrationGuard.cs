using Microsoft.Extensions.Options;
using SWFC.Application.M100_System.M102_Organization.Users;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M806_AccessControl.Assignments;
using SWFC.Application.M800_Security.M806_AccessControl.Shared;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Infrastructure.M100_System.M103_Authentication.Configuration;
using SWFC.Infrastructure.M100_System.M107_SetupDeployment;

namespace SWFC.Infrastructure.Services.Security;

public sealed class SecurityAdministrationGuard : ISecurityAdministrationGuard
{
    private readonly IUserReadRepository _userReadRepository;
    private readonly IUserRoleReadRepository _userRoleReadRepository;
    private readonly AuthenticationOptions _authenticationOptions;
    private readonly M107SetupOptions _initializationOptions;

    public SecurityAdministrationGuard(
        IUserReadRepository userReadRepository,
        IUserRoleReadRepository userRoleReadRepository,
        IOptions<AuthenticationOptions> authenticationOptions,
        IOptions<M107SetupOptions> initializationOptions)
    {
        _userReadRepository = userReadRepository;
        _userRoleReadRepository = userRoleReadRepository;
        _authenticationOptions = authenticationOptions.Value;
        _initializationOptions = initializationOptions.Value;
    }

    public async Task<Error?> ValidateRoleAssignmentAsync(
        SecurityContext actor,
        Guid targetUserId,
        string targetRoleName,
        CancellationToken cancellationToken = default)
    {
        if (actor.IsDeveloperMode)
        {
            return null;
        }

        if (!actor.HasRole(_initializationOptions.SuperAdminRoleName))
        {
            return new Error(
                "m806.assignment.forbidden",
                "Only SuperAdmin or Developer mode is allowed to change user roles.",
                ErrorCategory.Security);
        }

        if (await IsProtectedSuperAdminAsync(targetUserId, cancellationToken))
        {
            return new Error(
                "m806.assignment.protected_superadmin",
                "The protected SuperAdmin account can only be changed in Developer mode.",
                ErrorCategory.Security);
        }

        if (string.Equals(targetRoleName, _initializationOptions.SuperAdminRoleName, StringComparison.OrdinalIgnoreCase))
        {
            return new Error(
                "m806.assignment.superadmin.forbidden",
                "Only Developer mode is allowed to assign the SuperAdmin role.",
                ErrorCategory.Security);
        }

        if (string.Equals(targetRoleName, _initializationOptions.LegacyDeveloperRoleName, StringComparison.OrdinalIgnoreCase))
        {
            return new Error(
                "m806.assignment.developer.forbidden",
                "Developer is not assigned as a user role. Use Developer mode instead.",
                ErrorCategory.Security);
        }

        return null;
    }

    public async Task<Error?> ValidateRoleRemovalAsync(
        SecurityContext actor,
        Guid targetUserId,
        string targetRoleName,
        CancellationToken cancellationToken = default)
    {
        if (actor.IsDeveloperMode)
        {
            return null;
        }

        if (!actor.HasRole(_initializationOptions.SuperAdminRoleName))
        {
            return new Error(
                "m806.assignment.forbidden",
                "Only SuperAdmin or Developer mode is allowed to change user roles.",
                ErrorCategory.Security);
        }

        if (await IsProtectedSuperAdminAsync(targetUserId, cancellationToken))
        {
            return new Error(
                "m806.assignment.protected_superadmin",
                "The protected SuperAdmin account cannot be stripped from its protected role outside Developer mode.",
                ErrorCategory.Security);
        }

        if (string.Equals(targetRoleName, _initializationOptions.SuperAdminRoleName, StringComparison.OrdinalIgnoreCase))
        {
            return new Error(
                "m806.assignment.superadmin.forbidden",
                "Only Developer mode is allowed to remove the SuperAdmin role.",
                ErrorCategory.Security);
        }

        if (string.Equals(targetRoleName, _initializationOptions.LegacyDeveloperRoleName, StringComparison.OrdinalIgnoreCase))
        {
            return new Error(
                "m806.assignment.developer.forbidden",
                "Developer is not managed as a user role.",
                ErrorCategory.Security);
        }

        return null;
    }

    public async Task<Error?> ValidateAdminPasswordResetAsync(
        SecurityContext actor,
        Guid targetUserId,
        CancellationToken cancellationToken = default)
    {
        if (actor.IsDeveloperMode)
        {
            return null;
        }

        if (await IsProtectedSuperAdminAsync(targetUserId, cancellationToken))
        {
            return new Error(
                "m103.auth.protected_superadmin",
                "The protected SuperAdmin account changes its password itself or via Developer mode.",
                ErrorCategory.Security);
        }

        if (actor.HasRole(_initializationOptions.SuperAdminRoleName))
        {
            return null;
        }

        if (!actor.HasRole(_initializationOptions.AdminRoleName))
        {
            return new Error(
                "m103.auth.forbidden",
                "Current user is not allowed to set passwords for other users.",
                ErrorCategory.Security);
        }

        var targetRoleNames = await GetActiveRoleNamesAsync(targetUserId, cancellationToken);

        if (targetRoleNames.Any(x =>
                string.Equals(x, _initializationOptions.AdminRoleName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(x, _initializationOptions.SuperAdminRoleName, StringComparison.OrdinalIgnoreCase)))
        {
            return new Error(
                "m103.auth.privileged_target.forbidden",
                "Admins may only set passwords for non-privileged users.",
                ErrorCategory.Security);
        }

        return null;
    }

    public Task<IReadOnlyList<string>> GetActiveRoleNamesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return _userRoleReadRepository.GetActiveRoleNamesByUserIdAsync(userId, cancellationToken);
    }

    public async Task<bool> IsProtectedSuperAdminAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userReadRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return false;
        }

        var matchesIdentity = string.Equals(
            user.IdentityKey.Value,
            _initializationOptions.SuperAdminIdentityKey,
            StringComparison.OrdinalIgnoreCase);

        var matchesUsername = string.Equals(
            user.Username.Value,
            _authenticationOptions.InitialSuperAdmin.Username,
            StringComparison.OrdinalIgnoreCase);

        if (!matchesIdentity && !matchesUsername)
        {
            return false;
        }

        var roleNames = await GetActiveRoleNamesAsync(userId, cancellationToken);

        return roleNames.Any(x => string.Equals(x, _initializationOptions.SuperAdminRoleName, StringComparison.OrdinalIgnoreCase));
    }
}
