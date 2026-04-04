using System.Text.Json;
using SWFC.Application.M100_System.M102_Organization.Commands;
using SWFC.Application.M100_System.M102_Organization.Interfaces;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.Common.Results;
using SWFC.Domain.Common.ValueObjects;

namespace SWFC.Application.M100_System.M102_Organization.Handlers;

public sealed class RemoveRoleFromUserHandler : IUseCaseHandler<RemoveRoleFromUserCommand, bool>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserWriteRepository _userWriteRepository;
    private readonly IRoleReadRepository _roleReadRepository;
    private readonly IAuditService _auditService;

    public RemoveRoleFromUserHandler(
        ICurrentUserService currentUserService,
        IUserWriteRepository userWriteRepository,
        IRoleReadRepository roleReadRepository,
        IAuditService auditService)
    {
        _currentUserService = currentUserService;
        _userWriteRepository = userWriteRepository;
        _roleReadRepository = roleReadRepository;
        _auditService = auditService;
    }

    public async Task<Result<bool>> HandleAsync(
        RemoveRoleFromUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);

        var user = await _userWriteRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
        {
            return Result<bool>.Failure(
                new Error(
                    "m102.user.not_found",
                    "User was not found.",
                    ErrorCategory.NotFound));
        }

        var role = await _roleReadRepository.GetByIdAsync(command.RoleId, cancellationToken);
        if (role is null)
        {
            return Result<bool>.Failure(
                new Error(
                    "m102.role.not_found",
                    "Role was not found.",
                    ErrorCategory.NotFound));
        }

        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);
        user.RemoveRole(role.Id);

        await _auditService.WriteAsync(
            userId: securityContext.UserId,
            username: securityContext.Username,
            action: "RemoveRoleFromUser",
            entity: "UserRole",
            entityId: $"{user.Id}:{role.Id}",
            timestampUtc: changeContext.ChangedAtUtc,
            oldValues: JsonSerializer.Serialize(new
            {
                UserId = user.Id,
                RoleId = role.Id,
                RoleName = role.Name.Value
            }),
            newValues: null,
            cancellationToken: cancellationToken);

        await _userWriteRepository.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}