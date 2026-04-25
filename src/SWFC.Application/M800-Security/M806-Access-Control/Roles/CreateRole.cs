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

public sealed record CreateRoleCommand(
    string Name,
    string? Description,
    string Reason);

public sealed class CreateRoleValidator : ICommandValidator<CreateRoleCommand>
{
    public Task<ValidationResult> ValidateAsync(
        CreateRoleCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

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

public sealed class CreateRolePolicy : IAuthorizationPolicy<CreateRoleCommand>
{
    public AuthorizationRequirement GetRequirement(CreateRoleCommand request) =>
        new(requiredPermissions: new[] { "security.write" });
}

public sealed class CreateRoleHandler : IUseCaseHandler<CreateRoleCommand, Guid>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRoleWriteRepository _roleWriteRepository;
    private readonly IAuditService _auditService;

    public CreateRoleHandler(
        ICurrentUserService currentUserService,
        IRoleWriteRepository roleWriteRepository,
        IAuditService auditService)
    {
        _currentUserService = currentUserService;
        _roleWriteRepository = roleWriteRepository;
        _auditService = auditService;
    }

    public async Task<Result<Guid>> HandleAsync(
        CreateRoleCommand command,
        CancellationToken cancellationToken = default)
    {
        var existingRole = await _roleWriteRepository.GetByNameAsync(
            command.Name.Trim(),
            cancellationToken);

        if (existingRole is not null)
        {
            return Result<Guid>.Failure(
                new Error(
                    "m806.role.name.exists",
                    "A role with the same name already exists.",
                    ErrorCategory.Conflict));
        }

        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);

        if (!securityContext.IsDeveloperMode)
        {
            return Result<Guid>.Failure(
                new Error(
                    "m806.role.create.forbidden",
                    "Only Developer mode is allowed to create roles.",
                    ErrorCategory.Security));
        }

        var roleName = RoleName.Create(command.Name);
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);

        var role = Role.Create(
            roleName,
            command.Description,
            changeContext);

        await _roleWriteRepository.AddAsync(role, cancellationToken);

        await _auditService.WriteAsync(
            new AuditWriteRequest(
                ActorUserId: securityContext.UserId,
                ActorDisplayName: securityContext.DisplayName,
                Action: "RoleChanged",
                Module: "M806",
                ObjectType: "Role",
                ObjectId: role.Id.ToString(),
                TimestampUtc: changeContext.ChangedAtUtc,
                NewValues: JsonSerializer.Serialize(new
                {
                    ChangeType = "Created",
                    role.Id,
                    Name = role.Name.Value,
                    role.Description
                }),
                Reason: command.Reason),
            cancellationToken);

        await _roleWriteRepository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(role.Id);
    }
}
