using System.Text.Json;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M800_Security.M806_AccessControl.Roles;

namespace SWFC.Application.M800_Security.M806_AccessControl.Roles;

public sealed record UpdateRoleCommand(
    Guid Id,
    string Name,
    string? Description,
    string Reason);

public sealed class UpdateRoleValidator : ICommandValidator<UpdateRoleCommand>
{
    public Task<ValidationResult> ValidateAsync(
        UpdateRoleCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.Id == Guid.Empty)
        {
            result.Add("Id", "Role id is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            result.Add("Name", "Role name is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            result.Add("Reason", "Reason is required.");
        }

        return Task.FromResult(result);
    }
}

public sealed class UpdateRolePolicy : IAuthorizationPolicy<UpdateRoleCommand>
{
    public AuthorizationRequirement GetRequirement(UpdateRoleCommand request) =>
        new(requiredPermissions: new[] { "security.write" });
}

public sealed class UpdateRoleHandler : IUseCaseHandler<UpdateRoleCommand, bool>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRoleWriteRepository _roleWriteRepository;
    private readonly IAuditService _auditService;

    public UpdateRoleHandler(
        ICurrentUserService currentUserService,
        IRoleWriteRepository roleWriteRepository,
        IAuditService auditService)
    {
        _currentUserService = currentUserService;
        _roleWriteRepository = roleWriteRepository;
        _auditService = auditService;
    }

    public async Task<Result<bool>> HandleAsync(
        UpdateRoleCommand command,
        CancellationToken cancellationToken = default)
    {
        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);

        if (!securityContext.IsDeveloperMode)
        {
            return Result<bool>.Failure(
                new Error(
                    "m806.role.update.forbidden",
                    "Only Developer mode is allowed to change role master data.",
                    ErrorCategory.Security));
        }

        var role = await _roleWriteRepository.GetByIdAsync(command.Id, cancellationToken);

        if (role is null)
        {
            return Result<bool>.Failure(
                new Error(
                    "m806.role.not_found",
                    "Role was not found.",
                    ErrorCategory.NotFound));
        }

        var conflictingRole = await _roleWriteRepository.GetByNameAsync(
            command.Name.Trim(),
            cancellationToken);

        if (conflictingRole is not null && conflictingRole.Id != role.Id)
        {
            return Result<bool>.Failure(
                new Error(
                    "m806.role.name.exists",
                    "A role with the same name already exists.",
                    ErrorCategory.Conflict));
        }

        var normalizedName = command.Name.Trim();
        var normalizedDescription = string.IsNullOrWhiteSpace(command.Description)
            ? null
            : command.Description.Trim();

        if (role.IsSystemRole &&
            (!string.Equals(role.Name.Value, normalizedName, StringComparison.Ordinal) ||
             !string.Equals(role.Description, normalizedDescription, StringComparison.Ordinal)))
        {
            return Result<bool>.Failure(
                new Error(
                    "m806.role.system_role_immutable",
                    "System roles cannot be changed in their master data.",
                    ErrorCategory.Security));
        }

        var oldValues = JsonSerializer.Serialize(new
        {
            role.Id,
            Name = role.Name.Value,
            role.Description,
            role.IsActive,
            role.IsSystemRole
        });

        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);
        role.UpdateDetails(RoleName.Create(normalizedName), normalizedDescription, changeContext);

        await _auditService.WriteAsync(
            new AuditWriteRequest(
                ActorUserId: securityContext.UserId,
                ActorDisplayName: securityContext.DisplayName,
                Action: "RoleChanged",
                Module: "M806",
                ObjectType: "Role",
                ObjectId: role.Id.ToString(),
                TimestampUtc: changeContext.ChangedAtUtc,
                OldValues: oldValues,
                NewValues: JsonSerializer.Serialize(new
                {
                    ChangeType = "Updated",
                    role.Id,
                    Name = role.Name.Value,
                    role.Description,
                    role.IsActive,
                    role.IsSystemRole
                }),
                Reason: command.Reason),
            cancellationToken);

        await _roleWriteRepository.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
