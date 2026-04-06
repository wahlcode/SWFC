using System.Text.Json;
using SWFC.Application.M100_System.M102_Organization.Commands;
using SWFC.Application.M100_System.M102_Organization.Interfaces;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.Common.Results;
using SWFC.Domain.Common.ValueObjects;
using SWFC.Domain.M100_System.M102_Organization.ValueObjects;

namespace SWFC.Application.M100_System.M102_Organization.Handlers;

public sealed class UpdateUserHandler : IUseCaseHandler<UpdateUserCommand, bool>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserWriteRepository _userWriteRepository;
    private readonly IAuditService _auditService;

    public UpdateUserHandler(
        ICurrentUserService currentUserService,
        IUserWriteRepository userWriteRepository,
        IAuditService auditService)
    {
        _currentUserService = currentUserService;
        _userWriteRepository = userWriteRepository;
        _auditService = auditService;
    }

    public async Task<Result<bool>> HandleAsync(
        UpdateUserCommand command,
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

        var conflictingUser = await _userWriteRepository.GetByUsernameAsync(
            command.Username.Trim(),
            cancellationToken);

        if (conflictingUser is not null && conflictingUser.Id != user.Id)
        {
            return Result<bool>.Failure(
                new Error(
                    "m102.user.username.exists",
                    "A user with the same username already exists.",
                    ErrorCategory.Conflict));
        }

        var oldValues = JsonSerializer.Serialize(new
        {
            user.Id,
            IdentityKey = user.IdentityKey.Value,
            Username = user.Username.Value,
            DisplayName = user.DisplayName.Value,
            user.IsActive
        });

        var username = Username.Create(command.Username);
        var displayName = UserDisplayName.Create(command.DisplayName);
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);

        user.UpdateDetails(
            username,
            displayName,
            command.IsActive,
            changeContext);

        await _auditService.WriteAsync(
            userId: securityContext.UserId,
            username: securityContext.DisplayName,
            action: "UpdateUser",
            entity: "User",
            entityId: user.Id.ToString(),
            timestampUtc: changeContext.ChangedAtUtc,
            oldValues: oldValues,
            newValues: JsonSerializer.Serialize(new
            {
                user.Id,
                IdentityKey = user.IdentityKey.Value,
                Username = user.Username.Value,
                DisplayName = user.DisplayName.Value,
                user.IsActive,
                command.Reason
            }),
            cancellationToken: cancellationToken);

        await _userWriteRepository.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}