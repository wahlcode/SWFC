using System.Text.Json;
using SWFC.Application.M100_System.M102_Organization.Commands;
using SWFC.Application.M100_System.M102_Organization.Interfaces;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.Common.Results;
using SWFC.Domain.Common.ValueObjects;
using SWFC.Domain.M100_System.M102_Organization.Entities;
using SWFC.Domain.M100_System.M102_Organization.ValueObjects;

namespace SWFC.Application.M100_System.M102_Organization.Handlers;

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
        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);

        var existingRole = await _roleWriteRepository.GetByNameAsync(
            command.Name.Trim(),
            cancellationToken);

        if (existingRole is not null)
        {
            return Result<Guid>.Failure(
                new Error(
                    "m102.role.name.exists",
                    "A role with the same name already exists.",
                    ErrorCategory.Conflict));
        }

        var roleName = RoleName.Create(command.Name);
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);

        var role = Role.Create(
            roleName,
            command.Description,
            changeContext);

        await _roleWriteRepository.AddAsync(role, cancellationToken);

        await _auditService.WriteAsync(
            userId: securityContext.UserId,
            username: securityContext.Username,
            action: "CreateRole",
            entity: "Role",
            entityId: role.Id.ToString(),
            timestampUtc: changeContext.ChangedAtUtc,
            oldValues: null,
            newValues: JsonSerializer.Serialize(new
            {
                role.Id,
                Name = role.Name.Value,
                role.Description,
                command.Reason
            }),
            cancellationToken: cancellationToken);

        await _roleWriteRepository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(role.Id);
    }
}