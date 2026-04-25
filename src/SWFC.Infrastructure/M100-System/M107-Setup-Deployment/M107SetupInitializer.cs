using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M100_System.M102_Organization.Assignments;
using SWFC.Domain.M100_System.M102_Organization.OrganizationUnits;
using SWFC.Domain.M100_System.M102_Organization.Users;
using SWFC.Domain.M800_Security.M806_AccessControl.Assignments;
using SWFC.Domain.M800_Security.M806_AccessControl.RolePermissions;
using SWFC.Domain.M800_Security.M806_AccessControl.Roles;
using SWFC.Infrastructure.M100_System.M103_Authentication.Configuration;
using SWFC.Infrastructure.M100_System.M103_Authentication.Entities;
using SWFC.Infrastructure.M100_System.M103_Authentication.Services;
using SWFC.Infrastructure.M100_System.M107_SetupDeployment.Entities;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.M100_System.M107_SetupDeployment;

public sealed class M107SetupInitializer : IM107SetupInitializer
{
    private const string InitializationUserId = "system-setup";
    private const string DefaultPreferredCultureName = "en-US";

    private static readonly string[] AdminPermissionCodes =
    [
        "organization.read",
        "organization.write",
        "support.read",
        "support.write"
    ];

    private static readonly string[] SuperAdminPermissionCodes =
    [
        "organization.read",
        "organization.write",
        "security.read",
        "security.write",
        "support.read",
        "support.write"
    ];

    private readonly AppDbContext _dbContext;
    private readonly M107SetupOptions _options;
    private readonly AuthenticationOptions _authenticationOptions;
    private readonly PasswordHasher _passwordHasher;
    private readonly IAuditService _auditService;

    public M107SetupInitializer(
        AppDbContext dbContext,
        IOptions<M107SetupOptions> options,
        IOptions<AuthenticationOptions> authenticationOptions,
        IAuditService auditService)
    {
        _dbContext = dbContext;
        _options = options.Value;
        _authenticationOptions = authenticationOptions.Value;
        _passwordHasher = new PasswordHasher();
        _auditService = auditService;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var pendingMigrations = (await _dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).ToArray();
        var setupAuditItems = new List<SetupAuditItem>();
        var databaseReachable = false;

        try
        {
            databaseReachable = await _dbContext.Database.CanConnectAsync(cancellationToken);
        }
        catch
        {
            databaseReachable = false;
        }

        setupAuditItems.Add(
            new SetupAuditItem(
                Action: "DatabaseChecked",
                ObjectType: "DatabaseConnection",
                ObjectId: "startup",
                Reason: databaseReachable
                    ? "Database connectivity was verified during startup bootstrap."
                    : "Database connectivity check reported a not-yet-ready database before migration startup.",
                NewValues: JsonSerializer.Serialize(new
                {
                    CanConnect = databaseReachable
                })));

        if (pendingMigrations.Length > 0)
        {
            setupAuditItems.Add(
                new SetupAuditItem(
                    Action: "MigrationApplied",
                    ObjectType: "DatabaseMigration",
                    ObjectId: "startup",
                    Reason: "Pending migrations were applied during startup bootstrap.",
                    NewValues: JsonSerializer.Serialize(pendingMigrations)));
        }

        await _dbContext.Database.MigrateAsync(cancellationToken);

        var setupState = await EnsureSetupStateAsync(cancellationToken);
        setupState.MarkInProgress(DateTimeOffset.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        try
        {
        var changeContext = ChangeContext.Create(
            InitializationUserId,
            "M107 setup bootstrap");

        var adminRoleResult = await EnsureRoleAsync(
            _options.AdminRoleName,
            "System role for user management without full access.",
            isSystemRole: true,
            allowProtectedActivation: false,
            changeContext,
            setupAuditItems,
            cancellationToken);

        var superAdminRoleResult = await EnsureRoleAsync(
            _options.SuperAdminRoleName,
            "System role for user management including administrator approvals.",
            isSystemRole: true,
            allowProtectedActivation: true,
            changeContext,
            setupAuditItems,
            cancellationToken);

        if (adminRoleResult.WasCreated)
        {
            var changedPermissionCodes = await EnsureRolePermissionsByCodeAsync(
                adminRoleResult.Role.Id,
                AdminPermissionCodes,
                changeContext,
                cancellationToken);

            if (changedPermissionCodes.Count > 0)
            {
                setupAuditItems.Add(
                    new SetupAuditItem(
                        Action: "SecurityConfigChanged",
                        ObjectType: "RolePermissionBootstrap",
                        ObjectId: adminRoleResult.Role.Id.ToString(),
                        Reason: $"Initial permissions were assigned to '{adminRoleResult.Role.Name.Value}' during first-time setup.",
                        NewValues: JsonSerializer.Serialize(new
                        {
                            RoleId = adminRoleResult.Role.Id,
                            RoleName = adminRoleResult.Role.Name.Value,
                            PermissionCodes = changedPermissionCodes
                        })));
            }
        }

        if (superAdminRoleResult.WasCreated)
        {
            var changedPermissionCodes = await EnsureRolePermissionsByCodeAsync(
                superAdminRoleResult.Role.Id,
                SuperAdminPermissionCodes,
                changeContext,
                cancellationToken);

            if (changedPermissionCodes.Count > 0)
            {
                setupAuditItems.Add(
                    new SetupAuditItem(
                        Action: "SecurityConfigChanged",
                        ObjectType: "RolePermissionBootstrap",
                        ObjectId: superAdminRoleResult.Role.Id.ToString(),
                        Reason: $"Initial permissions were assigned to '{superAdminRoleResult.Role.Name.Value}' during first-time setup.",
                        NewValues: JsonSerializer.Serialize(new
                        {
                            RoleId = superAdminRoleResult.Role.Id,
                            RoleName = superAdminRoleResult.Role.Name.Value,
                            PermissionCodes = changedPermissionCodes
                        })));
            }
        }

        var superAdminUserResult = await EnsureProtectedSuperAdminUserAsync(changeContext, setupAuditItems, cancellationToken);
        var superAdminUser = superAdminUserResult.User;

        if (superAdminUserResult.WasCreated)
        {
            await EnsureSingleProtectedSuperAdminAssignmentAsync(
                superAdminUser.Id,
                superAdminRoleResult.Role.Id,
                changeContext,
                setupAuditItems,
                cancellationToken);
        }

        await RetireLegacyDeveloperArtifactsAsync(changeContext, setupAuditItems, cancellationToken);

        if (_options.CreateRootOrganizationUnit)
        {
            var root = await EnsureRootOrganizationUnitAsync(changeContext, setupAuditItems, cancellationToken);

            if (superAdminUserResult.WasCreated)
            {
                await EnsureUserOrganizationUnitAsync(
                    superAdminUser.Id,
                    root.Id,
                    isPrimary: true,
                    changeContext,
                    setupAuditItems,
                    cancellationToken);
            }
        }

        if (superAdminUserResult.WasCreated &&
            ShouldProvisionProtectedSuperAdminCredential())
        {
            await EnsureLocalCredentialAsync(
                superAdminUser.Id,
                _authenticationOptions.InitialSuperAdmin.Password,
                setupAuditItems,
                cancellationToken);
        }

        setupState.MarkCompleted(DateTimeOffset.UtcNow);

        await _dbContext.SaveChangesAsync(cancellationToken);

        if (setupAuditItems.Count > 0)
        {
            await WriteSetupAuditAsync(setupAuditItems, cancellationToken);
        }
        }
        catch (Exception ex)
        {
            setupState.MarkFailed(DateTimeOffset.UtcNow, ex.Message);
            await _dbContext.SaveChangesAsync(cancellationToken);
            throw;
        }
    }

    private async Task<SetupState> EnsureSetupStateAsync(CancellationToken cancellationToken)
    {
        var setupState = await _dbContext.SetupStates
            .FirstOrDefaultAsync(x => x.Key == SetupState.SystemStateKey, cancellationToken);

        if (setupState is not null)
        {
            return setupState;
        }

        setupState = SetupState.Create(DateTimeOffset.UtcNow);
        await _dbContext.SetupStates.AddAsync(setupState, cancellationToken);
        return setupState;
    }

    private async Task<RoleEnsureResult> EnsureRoleAsync(
        string roleName,
        string description,
        bool isSystemRole,
        bool allowProtectedActivation,
        ChangeContext changeContext,
        List<SetupAuditItem> setupAuditItems,
        CancellationToken cancellationToken)
    {
        var normalizedRoleName = roleName.Trim();
        var normalizedDescription = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();

        var existingRole = _dbContext.Roles
            .AsEnumerable()
            .FirstOrDefault(x => string.Equals(x.Name.Value, normalizedRoleName, StringComparison.OrdinalIgnoreCase));

        if (existingRole is not null)
        {
            if (allowProtectedActivation && !existingRole.IsActive)
            {
                existingRole.Activate(changeContext);

                setupAuditItems.Add(
                    new SetupAuditItem(
                        Action: "SecurityConfigChanged",
                        ObjectType: "Role",
                        ObjectId: existingRole.Id.ToString(),
                        Reason: $"Protected role '{normalizedRoleName}' was reactivated during startup bootstrap to keep the protected SuperAdmin reachable.",
                        NewValues: JsonSerializer.Serialize(new
                        {
                            existingRole.Id,
                            RoleName = existingRole.Name.Value,
                            existingRole.IsActive
                        })));
            }

            return new RoleEnsureResult(existingRole, WasCreated: false);
        }

        var role = Role.Create(
            RoleName.Create(normalizedRoleName),
            normalizedDescription,
            changeContext,
            isSystemRole);

        await _dbContext.Roles.AddAsync(role, cancellationToken);
        setupAuditItems.Add(
            new SetupAuditItem(
                Action: "SecurityConfigChanged",
                ObjectType: "Role",
                ObjectId: role.Id.ToString(),
                Reason: $"Missing system role '{normalizedRoleName}' was created during startup bootstrap.",
                NewValues: JsonSerializer.Serialize(new
                {
                    role.Id,
                    Name = role.Name.Value,
                    role.Description,
                    role.IsSystemRole
                })));

        return new RoleEnsureResult(role, WasCreated: true);
    }

    private async Task<IReadOnlyList<string>> EnsureRolePermissionsByCodeAsync(
        Guid roleId,
        IReadOnlyCollection<string> permissionCodes,
        ChangeContext changeContext,
        CancellationToken cancellationToken)
    {
        var normalizedCodes = new HashSet<string>(
            permissionCodes.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()),
            StringComparer.OrdinalIgnoreCase);

        var desiredPermissions = await _dbContext.Permissions
            .Where(x => x.IsActive && normalizedCodes.Contains(x.Code))
            .Select(x => new PermissionBootstrapItem(x.Id, x.Code))
            .ToListAsync(cancellationToken);

        return await AddMissingRolePermissionsAsync(roleId, desiredPermissions, changeContext, cancellationToken);
    }

    private async Task<IReadOnlyList<string>> AddMissingRolePermissionsAsync(
        Guid roleId,
        IReadOnlyCollection<PermissionBootstrapItem> desiredPermissions,
        ChangeContext changeContext,
        CancellationToken cancellationToken)
    {
        var desiredPermissionSet = desiredPermissions
            .Select(x => x.Id)
            .Where(x => x != Guid.Empty)
            .ToHashSet();
        var permissionCodesById = desiredPermissions.ToDictionary(x => x.Id, x => x.Code);
        var changedPermissionCodes = new List<string>();

        var existingAssignments = await _dbContext.RolePermissions
            .Where(x => x.RoleId == roleId)
            .ToListAsync(cancellationToken);

        foreach (var assignment in existingAssignments.Where(x => desiredPermissionSet.Contains(x.PermissionId)))
        {
            if (assignment.IsActive)
            {
                continue;
            }

            assignment.Activate(changeContext);
            changedPermissionCodes.Add(permissionCodesById[assignment.PermissionId]);
        }

        var existingPermissionIds = existingAssignments
            .Select(x => x.PermissionId)
            .ToHashSet();

        foreach (var permission in desiredPermissions.Where(x => !existingPermissionIds.Contains(x.Id)))
        {
            await _dbContext.RolePermissions.AddAsync(
                RolePermission.Create(roleId, permission.Id, changeContext),
                cancellationToken);
            changedPermissionCodes.Add(permission.Code);
        }

        return changedPermissionCodes;
    }

    private async Task<UserEnsureResult> EnsureProtectedSuperAdminUserAsync(
        ChangeContext changeContext,
        List<SetupAuditItem> setupAuditItems,
        CancellationToken cancellationToken)
    {
        var identityKey = _options.SuperAdminIdentityKey.Trim();
        var username = string.IsNullOrWhiteSpace(_authenticationOptions.InitialSuperAdmin.Username)
            ? "swfc.dev"
            : _authenticationOptions.InitialSuperAdmin.Username.Trim();

        var existingUser = _dbContext.Users
            .Include(x => x.OrganizationUnits)
            .AsEnumerable()
            .FirstOrDefault(x =>
                string.Equals(x.IdentityKey.Value, identityKey, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(x.Username.Value, username, StringComparison.OrdinalIgnoreCase));

        if (existingUser is not null)
        {
            return new UserEnsureResult(existingUser, WasCreated: false);
        }

        var user = User.Create(
            UserIdentityKey.Create(identityKey),
            Username.Create(username),
            UserDisplayName.Create(_options.SuperAdminDisplayName),
            _options.SuperAdminFirstName,
            _options.SuperAdminLastName,
            _options.SuperAdminEmployeeNumber,
            _options.SuperAdminBusinessEmail,
            _options.SuperAdminBusinessPhone,
            _options.SuperAdminPlant,
            _options.SuperAdminLocation,
            _options.SuperAdminTeam,
            costCenterId: null,
            shiftModelId: null,
            _options.SuperAdminJobFunction,
            DefaultPreferredCultureName,
            UserStatus.Active,
            UserType.Internal,
            changeContext);

        await _dbContext.Users.AddAsync(user, cancellationToken);
        setupAuditItems.Add(
            new SetupAuditItem(
                Action: "UserChanged",
                ObjectType: "User",
                ObjectId: user.Id.ToString(),
                Reason: "Protected SuperAdmin account was created during startup bootstrap.",
                NewValues: JsonSerializer.Serialize(new
                {
                    user.Id,
                    IdentityKey = user.IdentityKey.Value,
                    Username = user.Username.Value,
                    DisplayName = user.DisplayName.Value,
                    Status = user.Status.ToString(),
                    user.PreferredCultureName
                })));
        return new UserEnsureResult(user, WasCreated: true);
    }

    private async Task EnsureSingleProtectedSuperAdminAssignmentAsync(
        Guid protectedSuperAdminUserId,
        Guid superAdminRoleId,
        ChangeContext changeContext,
        List<SetupAuditItem> setupAuditItems,
        CancellationToken cancellationToken)
    {
        var assignments = await _dbContext.UserRoles
            .Where(x => x.RoleId == superAdminRoleId)
            .ToListAsync(cancellationToken);
        var assignmentChanges = new List<object>();

        foreach (var assignment in assignments)
        {
            if (assignment.UserId == protectedSuperAdminUserId)
            {
                if (!assignment.IsActive)
                {
                    assignment.Activate(changeContext);
                    assignmentChanges.Add(new
                    {
                        assignment.UserId,
                        assignment.RoleId,
                        Change = "ActivatedProtectedSuperAdminAssignment"
                    });
                }

                continue;
            }

            if (!assignment.IsActive)
            {
                continue;
            }

            assignment.Deactivate(changeContext);
            assignmentChanges.Add(new
            {
                assignment.UserId,
                assignment.RoleId,
                Change = "DeactivatedForeignSuperAdminAssignment"
            });
        }

        if (assignments.All(x => x.UserId != protectedSuperAdminUserId))
        {
            await _dbContext.UserRoles.AddAsync(
                UserRole.Create(protectedSuperAdminUserId, superAdminRoleId, changeContext),
                cancellationToken);
            assignmentChanges.Add(new
            {
                UserId = protectedSuperAdminUserId,
                RoleId = superAdminRoleId,
                Change = "CreatedProtectedSuperAdminAssignment"
            });
        }

        if (assignmentChanges.Count > 0)
        {
            setupAuditItems.Add(
                new SetupAuditItem(
                    Action: "UserRoleChanged",
                    ObjectType: "UserRoleAssignment",
                    ObjectId: superAdminRoleId.ToString(),
                    Reason: "Protected SuperAdmin exclusivity was enforced during startup bootstrap.",
                    NewValues: JsonSerializer.Serialize(assignmentChanges)));
        }
    }

    private async Task RetireLegacyDeveloperArtifactsAsync(
        ChangeContext changeContext,
        List<SetupAuditItem> setupAuditItems,
        CancellationToken cancellationToken)
    {
        var changes = new List<object>();

        var developerRole = _dbContext.Roles
            .AsEnumerable()
            .FirstOrDefault(x => string.Equals(x.Name.Value, _options.LegacyDeveloperRoleName, StringComparison.OrdinalIgnoreCase));

        if (developerRole is not null)
        {
            if (developerRole.IsActive)
            {
                developerRole.Deactivate(changeContext);
                changes.Add(new
                {
                    ObjectType = "Role",
                    developerRole.Id,
                    RoleName = developerRole.Name.Value,
                    Change = "Deactivated"
                });
            }

            var developerRoleAssignments = await _dbContext.UserRoles
                .Where(x => x.RoleId == developerRole.Id && x.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var assignment in developerRoleAssignments)
            {
                assignment.Deactivate(changeContext);
                changes.Add(new
                {
                    ObjectType = "UserRoleAssignment",
                    assignment.UserId,
                    assignment.RoleId,
                    Change = "Deactivated"
                });
            }
        }

        var legacyDeveloperUser = _dbContext.Users
            .Include(x => x.OrganizationUnits)
            .AsEnumerable()
            .FirstOrDefault(x =>
                string.Equals(x.IdentityKey.Value, _options.LegacyDeveloperIdentityKey, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(x.Username.Value, _authenticationOptions.LegacyDeveloper.Username, StringComparison.OrdinalIgnoreCase));

        if (legacyDeveloperUser is null)
        {
            return;
        }

        if (legacyDeveloperUser.Status != UserStatus.Inactive)
        {
            legacyDeveloperUser.ChangeStatus(UserStatus.Inactive, changeContext);
            changes.Add(new
            {
                ObjectType = "User",
                legacyDeveloperUser.Id,
                Username = legacyDeveloperUser.Username.Value,
                Change = "StatusSetToInactive"
            });
        }

        var credential = await _dbContext.LocalCredentials
            .FirstOrDefaultAsync(x => x.UserId == legacyDeveloperUser.Id, cancellationToken);

        if (credential is not null && credential.IsActive)
        {
            credential.Deactivate();
            changes.Add(new
            {
                ObjectType = "LocalCredential",
                credential.UserId,
                Change = "Deactivated"
            });
        }

        if (changes.Count > 0)
        {
            setupAuditItems.Add(
                new SetupAuditItem(
                    Action: "SecurityConfigChanged",
                    ObjectType: "LegacyDeveloperRetirement",
                    ObjectId: legacyDeveloperUser.Id.ToString(),
                    Reason: "Legacy developer artifacts were retired because Developer now exists only as a protected mode.",
                    NewValues: JsonSerializer.Serialize(changes)));
        }
    }

    private async Task<OrganizationUnit> EnsureRootOrganizationUnitAsync(
        ChangeContext changeContext,
        List<SetupAuditItem> setupAuditItems,
        CancellationToken cancellationToken)
    {
        var rootCode = _options.RootOrganizationUnitCode.Trim();

        var existingOrganizationUnit = _dbContext.OrganizationUnits
            .AsEnumerable()
            .FirstOrDefault(x => string.Equals(x.Code.Value, rootCode, StringComparison.OrdinalIgnoreCase));

        if (existingOrganizationUnit is not null)
        {
            return existingOrganizationUnit;
        }

        var organizationUnit = OrganizationUnit.Create(
            OrganizationUnitName.Create(_options.RootOrganizationUnitName),
            OrganizationUnitCode.Create(rootCode),
            parentOrganizationUnitId: null,
            changeContext);

        await _dbContext.OrganizationUnits.AddAsync(organizationUnit, cancellationToken);
        setupAuditItems.Add(
            new SetupAuditItem(
                Action: "SetupConfigChanged",
                ObjectType: "OrganizationUnit",
                ObjectId: organizationUnit.Id.ToString(),
                Reason: "Missing root organization unit was created during startup bootstrap.",
                NewValues: JsonSerializer.Serialize(new
                {
                    organizationUnit.Id,
                    Name = organizationUnit.Name.Value,
                    Code = organizationUnit.Code.Value
                })));
        return organizationUnit;
    }

    private async Task EnsureUserOrganizationUnitAsync(
        Guid userId,
        Guid organizationUnitId,
        bool isPrimary,
        ChangeContext changeContext,
        List<SetupAuditItem> setupAuditItems,
        CancellationToken cancellationToken)
    {
        var existingAssignments = await _dbContext.UserOrganizationUnits
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);
        var assignmentChanges = new List<object>();

        var existingAssignment = existingAssignments
            .FirstOrDefault(x => x.OrganizationUnitId == organizationUnitId);

        if (existingAssignment is not null)
        {
            var wasActive = existingAssignment.IsActive;
            var wasPrimary = existingAssignment.IsPrimary;

            if (!wasActive || (isPrimary && !wasPrimary))
            {
                existingAssignment.Activate(isPrimary, changeContext);
                assignmentChanges.Add(new
                {
                    userId,
                    organizationUnitId,
                    Change = !wasActive
                        ? "ActivatedProtectedSuperAdminOrganization"
                        : "UpdatedProtectedSuperAdminPrimaryOrganization"
                });
            }

            if (isPrimary && !wasPrimary)
            {
                foreach (var assignment in existingAssignments.Where(x => x.IsPrimary && x.OrganizationUnitId != organizationUnitId))
                {
                    assignment.SetPrimary(false, changeContext);
                    assignmentChanges.Add(new
                    {
                        assignment.UserId,
                        assignment.OrganizationUnitId,
                        Change = "RemovedPrimaryFlagFromForeignAssignment"
                    });
                }

                existingAssignment.SetPrimary(true, changeContext);
            }

            if (assignmentChanges.Count > 0)
            {
                setupAuditItems.Add(
                    new SetupAuditItem(
                        Action: "UserChanged",
                        ObjectType: "UserOrganizationAssignment",
                        ObjectId: $"{userId:N}:{organizationUnitId:N}",
                        Reason: "Protected SuperAdmin organization context was ensured during startup bootstrap.",
                        NewValues: JsonSerializer.Serialize(assignmentChanges)));
            }

            return;
        }

        if (isPrimary)
        {
            foreach (var assignment in existingAssignments.Where(x => x.IsPrimary))
            {
                assignment.SetPrimary(false, changeContext);
                assignmentChanges.Add(new
                {
                    assignment.UserId,
                    assignment.OrganizationUnitId,
                    Change = "RemovedPrimaryFlagFromForeignAssignment"
                });
            }
        }

        var userOrganizationUnit = UserOrganizationUnit.Create(
            userId,
            organizationUnitId,
            isPrimary,
            changeContext);

        await _dbContext.UserOrganizationUnits.AddAsync(userOrganizationUnit, cancellationToken);
        assignmentChanges.Add(new
        {
            userId,
            organizationUnitId,
            Change = "CreatedProtectedSuperAdminOrganizationAssignment",
            isPrimary
        });
        setupAuditItems.Add(
            new SetupAuditItem(
                Action: "UserChanged",
                ObjectType: "UserOrganizationAssignment",
                ObjectId: $"{userId:N}:{organizationUnitId:N}",
                Reason: "Protected SuperAdmin organization context was created during startup bootstrap.",
                NewValues: JsonSerializer.Serialize(assignmentChanges)));
    }

    private async Task EnsureLocalCredentialAsync(
        Guid userId,
        string? rawPassword,
        List<SetupAuditItem> setupAuditItems,
        CancellationToken cancellationToken)
    {
        var initialPassword = rawPassword?.Trim();
        var existingCredential = await _dbContext.LocalCredentials
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        if (existingCredential is not null)
        {
            var credentialChanges = new List<string>();

            if (!existingCredential.IsActive)
            {
                existingCredential.Activate();
                credentialChanges.Add("Reactivated");
            }

            if (existingCredential.FailedAttempts > 0 || existingCredential.LockoutUntilUtc.HasValue)
            {
                existingCredential.ResetFailedAttempts();
                credentialChanges.Add("ClearedLockoutState");
            }

            if (!string.IsNullOrWhiteSpace(initialPassword) &&
                !_passwordHasher.VerifyPassword(initialPassword, existingCredential.PasswordHash))
            {
                existingCredential.ChangePassword(
                    _passwordHasher.HashPassword(initialPassword),
                    DateTimeOffset.UtcNow);
                existingCredential.Activate();
                credentialChanges.Add("SynchronizedRuntimePassword");
            }

            if (credentialChanges.Count > 0)
            {
                setupAuditItems.Add(
                    new SetupAuditItem(
                        Action: "PasswordSet",
                        ObjectType: "LocalCredential",
                        ObjectId: userId.ToString(),
                        Reason: "Protected SuperAdmin local credential was normalized during startup bootstrap.",
                        NewValues: JsonSerializer.Serialize(new
                        {
                            existingCredential.UserId,
                            existingCredential.IsActive,
                            existingCredential.LastPasswordChangedAtUtc,
                            Changes = credentialChanges
                        })));
            }

            return;
        }

        if (string.IsNullOrWhiteSpace(initialPassword))
        {
            throw new InvalidOperationException(
                "The protected SuperAdmin password must be provided via configuration or environment before the first startup.");
        }

        var passwordHash = _passwordHasher.HashPassword(initialPassword);

        var credential = LocalCredential.Create(
            userId,
            passwordHash,
            DateTimeOffset.UtcNow);

        await _dbContext.LocalCredentials.AddAsync(credential, cancellationToken);
        setupAuditItems.Add(
            new SetupAuditItem(
                Action: "PasswordSet",
                ObjectType: "LocalCredential",
                ObjectId: userId.ToString(),
                Reason: "Protected SuperAdmin local credential was created during startup bootstrap.",
                NewValues: JsonSerializer.Serialize(new
                {
                    credential.UserId,
                    credential.IsActive,
                    credential.LastPasswordChangedAtUtc
                })));
    }

    private Task WriteSetupAuditAsync(
        IReadOnlyCollection<SetupAuditItem> setupAuditItems,
        CancellationToken cancellationToken)
    {
        return _auditService.WriteAsync(
            new AuditWriteRequest(
                ActorUserId: InitializationUserId,
                ActorDisplayName: "SWFC Setup Bootstrap",
                Action: "SetupConfigChanged",
                Module: "M107",
                ObjectType: "SystemBootstrap",
                ObjectId: "startup",
                TimestampUtc: DateTime.UtcNow,
                NewValues: JsonSerializer.Serialize(setupAuditItems),
                Reason: "Startup bootstrap ensured the documented baseline state without resetting productive security data."),
            cancellationToken);
    }

    private bool ShouldProvisionProtectedSuperAdminCredential()
    {
        if (string.Equals(_authenticationOptions.Mode, AuthenticationModes.Local, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return string.Equals(_authenticationOptions.Mode, AuthenticationModes.Sso, StringComparison.OrdinalIgnoreCase) &&
               !string.IsNullOrWhiteSpace(_authenticationOptions.InitialSuperAdmin.Password);
    }

    private sealed record PermissionBootstrapItem(Guid Id, string Code);
    private sealed record RoleEnsureResult(Role Role, bool WasCreated);
    private sealed record UserEnsureResult(User User, bool WasCreated);
    private sealed record SetupAuditItem(
        string Action,
        string ObjectType,
        string ObjectId,
        string Reason,
        string? OldValues = null,
        string? NewValues = null);
}
