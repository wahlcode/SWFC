using System.Text.Json;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Application.M800_Security.M806_AccessControl.Roles;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M800_Security.M806_AccessControl.RolePermissions;

public sealed class SetRolePermissionsValidator : ICommandValidator<SetRolePermissionsCommand>
{
    public Task<ValidationResult> ValidateAsync(
        SetRolePermissionsCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.RoleId == Guid.Empty)
        {
            result.Add("RoleId", "Role id is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            result.Add("Reason", "Reason is required.");
        }

        if (command.PermissionIds is null)
        {
            result.Add("PermissionIds", "Permission ids are required.");
        }

        return Task.FromResult(result);
    }
}

public sealed class SetRolePermissionsPolicy : IAuthorizationPolicy<SetRolePermissionsCommand>
{
    public AuthorizationRequirement GetRequirement(SetRolePermissionsCommand request) =>
        new(requiredPermissions: new[] { "security.write" });
}

public sealed class SetRolePermissionsHandler : IUseCaseHandler<SetRolePermissionsCommand, bool>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRoleReadRepository _roleReadRepository;
    private readonly IRolePermissionReadRepository _rolePermissionReadRepository;
    private readonly IRolePermissionWriteRepository _rolePermissionWriteRepository;
    private readonly IAuditService _auditService;

    public SetRolePermissionsHandler(
        ICurrentUserService currentUserService,
        IRoleReadRepository roleReadRepository,
        IRolePermissionReadRepository rolePermissionReadRepository,
        IRolePermissionWriteRepository rolePermissionWriteRepository,
        IAuditService auditService)
    {
        _currentUserService = currentUserService;
        _roleReadRepository = roleReadRepository;
        _rolePermissionReadRepository = rolePermissionReadRepository;
        _rolePermissionWriteRepository = rolePermissionWriteRepository;
        _auditService = auditService;
    }

    public async Task<Result<bool>> HandleAsync(
        SetRolePermissionsCommand command,
        CancellationToken cancellationToken = default)
    {
        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);

        if (!securityContext.IsDeveloperMode)
        {
            return Result<bool>.Failure(
                new Error(
                    "m806.role_permissions.forbidden",
                    "Only Developer mode is allowed to change role permissions.",
                    ErrorCategory.Security));
        }

        var role = await _roleReadRepository.GetByIdAsync(command.RoleId, cancellationToken);

        if (role is null)
        {
            return Result<bool>.Failure(
                new Error(
                    "m806.role_permissions.role.not_found",
                    "Role was not found.",
                    ErrorCategory.NotFound));
        }

        var previousAssignments = await _rolePermissionReadRepository.GetByRoleIdAsync(
            command.RoleId,
            cancellationToken);

        await _rolePermissionWriteRepository.SetPermissionsAsync(
            command.RoleId,
            command.PermissionIds,
            securityContext.UserId,
            cancellationToken);

        var currentAssignments = await _rolePermissionReadRepository.GetByRoleIdAsync(
            command.RoleId,
            cancellationToken);

        await _auditService.WriteAsync(
            new AuditWriteRequest(
                ActorUserId: securityContext.UserId,
                ActorDisplayName: securityContext.DisplayName,
                Action: "PermissionChanged",
                Module: "M806",
                ObjectType: "RolePermissionAssignment",
                ObjectId: command.RoleId.ToString(),
                TimestampUtc: DateTime.UtcNow,
                OldValues: JsonSerializer.Serialize(new
                {
                    RoleId = role.Id,
                    RoleName = role.Name.Value,
                    Permissions = previousAssignments
                        .Where(x => x.IsAssigned)
                        .Select(x => new
                        {
                            x.PermissionId,
                            x.PermissionCode,
                            x.PermissionName,
                            x.Module
                        })
                }),
                NewValues: JsonSerializer.Serialize(new
                {
                    RoleId = role.Id,
                    RoleName = role.Name.Value,
                    Permissions = currentAssignments
                        .Where(x => x.IsAssigned)
                        .Select(x => new
                        {
                            x.PermissionId,
                            x.PermissionCode,
                            x.PermissionName,
                            x.Module
                        })
                }),
                Reason: command.Reason),
            cancellationToken);

        return Result<bool>.Success(true);
    }
}
