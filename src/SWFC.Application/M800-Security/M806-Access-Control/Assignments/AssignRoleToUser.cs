using System.Text.Json;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Application.M100_System.M102_Organization.Users;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Application.M800_Security.M806_AccessControl.Roles;
using SWFC.Application.M800_Security.M806_AccessControl.Shared;

namespace SWFC.Application.M800_Security.M806_AccessControl.Assignments;

public sealed record AssignRoleToUserCommand(
    Guid UserId,
    Guid RoleId,
    string Reason);

public sealed class AssignRoleToUserValidator : ICommandValidator<AssignRoleToUserCommand>
{
    public Task<ValidationResult> ValidateAsync(
        AssignRoleToUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.UserId == Guid.Empty)
        {
            result.Add("UserId", "User id is required.");
        }

        if (command.RoleId == Guid.Empty)
        {
            result.Add("RoleId", "Role id is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            result.Add("Reason", "Reason is required.");
        }

        return Task.FromResult(result);
    }
}

public sealed class AssignRoleToUserPolicy : IAuthorizationPolicy<AssignRoleToUserCommand>
{
    public AuthorizationRequirement GetRequirement(AssignRoleToUserCommand request) =>
        new(requiredPermissions: new[] { "security.write" });
}

public sealed class AssignRoleToUserHandler : IUseCaseHandler<AssignRoleToUserCommand, bool>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRoleAssignmentWriteRepository _userRoleAssignmentWriteRepository;
    private readonly IUserReadRepository _userReadRepository;
    private readonly IRoleReadRepository _roleReadRepository;
    private readonly ISecurityAdministrationGuard _securityAdministrationGuard;
    private readonly IAuditService _auditService;

    public AssignRoleToUserHandler(
        ICurrentUserService currentUserService,
        IUserRoleAssignmentWriteRepository userRoleAssignmentWriteRepository,
        IUserReadRepository userReadRepository,
        IRoleReadRepository roleReadRepository,
        ISecurityAdministrationGuard securityAdministrationGuard,
        IAuditService auditService)
    {
        _currentUserService = currentUserService;
        _userRoleAssignmentWriteRepository = userRoleAssignmentWriteRepository;
        _userReadRepository = userReadRepository;
        _roleReadRepository = roleReadRepository;
        _securityAdministrationGuard = securityAdministrationGuard;
        _auditService = auditService;
    }

    public async Task<Result<bool>> HandleAsync(
        AssignRoleToUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);
        var role = await _roleReadRepository.GetByIdAsync(command.RoleId, cancellationToken);

        if (role is null)
        {
            return Result<bool>.Failure(
                new Error(
                    "m806.assignment.role.not_found",
                    "Role was not found.",
                    ErrorCategory.NotFound));
        }

        var roleAssignmentError = await _securityAdministrationGuard.ValidateRoleAssignmentAsync(
            securityContext,
            command.UserId,
            role.Name.Value,
            cancellationToken);

        if (roleAssignmentError is not null)
        {
            return Result<bool>.Failure(roleAssignmentError);
        }

        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);

        var changed = await _userRoleAssignmentWriteRepository.AssignRoleAsync(
            command.UserId,
            command.RoleId,
            changeContext,
            cancellationToken);

        if (!changed)
        {
            return Result<bool>.Failure(
                new Error(
                    "m806.assignment.assign_role.failed",
                    "Role could not be assigned to user.",
                    ErrorCategory.Validation));
        }

        var user = await _userReadRepository.GetByIdAsync(command.UserId, cancellationToken);

        await _auditService.WriteAsync(
                new AuditWriteRequest(
                    ActorUserId: securityContext.UserId,
                    ActorDisplayName: securityContext.DisplayName,
                    Action: "UserRoleChanged",
                    Module: "M806",
                    ObjectType: "UserRoleAssignment",
                    ObjectId: $"{command.UserId:N}:{command.RoleId:N}",
                TimestampUtc: changeContext.ChangedAtUtc,
                NewValues: JsonSerializer.Serialize(new
                {
                    ChangeType = "Assigned",
                    UserId = command.UserId,
                    Username = user?.Username.Value,
                    RoleId = command.RoleId,
                    RoleName = role?.Name.Value
                }),
                TargetUserId: command.UserId.ToString(),
                Reason: command.Reason),
            cancellationToken);

        return Result<bool>.Success(true);
    }
}
