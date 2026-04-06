using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SWFC.Domain.Common.ValueObjects;
using SWFC.Domain.M100_System.M102_Organization.Entities;
using SWFC.Domain.M100_System.M102_Organization.ValueObjects;
using SWFC.Infrastructure.M800_Security.Auth.Configuration;
using SWFC.Infrastructure.M800_Security.Auth.Entities;
using SWFC.Infrastructure.M800_Security.Auth.Services;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Services.System;

public sealed class M102DataInitializer : IM102DataInitializer
{
    private const string InitializationUserId = "system-initializer";
    private readonly AppDbContext _dbContext;
    private readonly M102InitializationOptions _options;
    private readonly AuthenticationOptions _authenticationOptions;
    private readonly PasswordHasher _passwordHasher;

    public M102DataInitializer(
        AppDbContext dbContext,
        IOptions<M102InitializationOptions> options,
        IOptions<AuthenticationOptions> authenticationOptions)
    {
        _dbContext = dbContext;
        _options = options.Value;
        _authenticationOptions = authenticationOptions.Value;
        _passwordHasher = new PasswordHasher();
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.Database.MigrateAsync(cancellationToken);

        var changeContext = ChangeContext.Create(
            InitializationUserId,
            "M102 startup initialization");

        var adminRole = await EnsureAdminRoleAsync(changeContext, cancellationToken);
        var developerUser = await EnsureDeveloperUserAsync(changeContext, cancellationToken);

        await EnsureUserRoleAsync(
            developerUser.Id,
            adminRole.Id,
            changeContext,
            cancellationToken);

        if (_options.CreateRootOrganizationUnit)
        {
            var root = await EnsureRootOrganizationUnitAsync(changeContext, cancellationToken);

            await EnsureUserOrganizationUnitAsync(
                developerUser.Id,
                root.Id,
                isPrimary: true,
                changeContext,
                cancellationToken);
        }

        await EnsureInitialAdminLocalCredentialAsync(developerUser.Id, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Role> EnsureAdminRoleAsync(
        ChangeContext changeContext,
        CancellationToken cancellationToken)
    {
        var roleName = _options.AdminRoleName.Trim();

        var existingRole = _dbContext.Roles
            .AsEnumerable()
            .FirstOrDefault(x => string.Equals(x.Name.Value, roleName, StringComparison.OrdinalIgnoreCase));

        if (existingRole is not null)
        {
            return existingRole;
        }

        var role = Role.Create(
            RoleName.Create(roleName),
            "Initial full access role",
            changeContext);

        await _dbContext.Roles.AddAsync(role, cancellationToken);

        return role;
    }

    private async Task<User> EnsureDeveloperUserAsync(
        ChangeContext changeContext,
        CancellationToken cancellationToken)
    {
        var identityKey = _options.DeveloperIdentityKey.Trim();

        var existingUser = _dbContext.Users
            .Include(x => x.Roles)
            .Include(x => x.OrganizationUnits)
            .AsEnumerable()
            .FirstOrDefault(x => string.Equals(x.IdentityKey.Value, identityKey, StringComparison.OrdinalIgnoreCase));

        if (existingUser is not null)
        {
            return existingUser;
        }

        var user = User.Create(
            UserIdentityKey.Create(identityKey),
            Username.Create(identityKey),
            UserDisplayName.Create(_options.DeveloperDisplayName),
            true,
            changeContext);

        await _dbContext.Users.AddAsync(user, cancellationToken);

        return user;
    }

    private async Task EnsureUserRoleAsync(
        Guid userId,
        Guid roleId,
        ChangeContext changeContext,
        CancellationToken cancellationToken)
    {
        var exists = await _dbContext.UserRoles
            .AnyAsync(
                x => x.UserId == userId && x.RoleId == roleId,
                cancellationToken);

        if (exists)
        {
            return;
        }

        var userRole = UserRole.Create(userId, roleId, changeContext);

        await _dbContext.UserRoles.AddAsync(userRole, cancellationToken);
    }

    private async Task<OrganizationUnit> EnsureRootOrganizationUnitAsync(
        ChangeContext changeContext,
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

        return organizationUnit;
    }

    private async Task EnsureUserOrganizationUnitAsync(
        Guid userId,
        Guid organizationUnitId,
        bool isPrimary,
        ChangeContext changeContext,
        CancellationToken cancellationToken)
    {
        var existingAssignments = await _dbContext.UserOrganizationUnits
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

        var existingAssignment = existingAssignments
            .FirstOrDefault(x => x.OrganizationUnitId == organizationUnitId);

        if (existingAssignment is not null)
        {
            if (isPrimary && !existingAssignment.IsPrimary)
            {
                foreach (var assignment in existingAssignments.Where(x => x.IsPrimary))
                {
                    assignment.SetPrimary(false, changeContext);
                }

                existingAssignment.SetPrimary(true, changeContext);
            }

            return;
        }

        if (isPrimary)
        {
            foreach (var assignment in existingAssignments.Where(x => x.IsPrimary))
            {
                assignment.SetPrimary(false, changeContext);
            }
        }

        var userOrganizationUnit = UserOrganizationUnit.Create(
            userId,
            organizationUnitId,
            isPrimary,
            changeContext);

        await _dbContext.UserOrganizationUnits.AddAsync(userOrganizationUnit, cancellationToken);
    }

    private async Task EnsureInitialAdminLocalCredentialAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var existingCredential = await _dbContext.LocalCredentials
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        if (existingCredential is not null)
        {
            return;
        }

        var initialPassword = _authenticationOptions.InitialAdmin.Password?.Trim();

        if (string.IsNullOrWhiteSpace(initialPassword))
        {
            return;
        }

        var passwordHash = _passwordHasher.HashPassword(initialPassword);

        var credential = LocalCredential.Create(
            userId,
            passwordHash,
            DateTimeOffset.UtcNow);

        await _dbContext.LocalCredentials.AddAsync(credential, cancellationToken);
    }
}