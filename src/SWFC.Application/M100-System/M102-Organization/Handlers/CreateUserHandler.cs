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

public sealed class CreateUserHandler : IUseCaseHandler<CreateUserCommand, Guid>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserWriteRepository _userWriteRepository;
    private readonly IAuditService _auditService;

    public CreateUserHandler(
        ICurrentUserService currentUserService,
        IUserWriteRepository userWriteRepository,
        IAuditService auditService)
    {
        _currentUserService = currentUserService;
        _userWriteRepository = userWriteRepository;
        _auditService = auditService;
    }

    public async Task<Result<Guid>> HandleAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);

        var existingIdentityKeyUser = await _userWriteRepository.GetByIdentityKeyAsync(
            command.IdentityKey.Trim(),
            cancellationToken);

        if (existingIdentityKeyUser is not null)
        {
            return Result<Guid>.Failure(
                new Error(
                    "m102.user.identity_key.exists",
                    "A user with the same identity key already exists.",
                    ErrorCategory.Conflict));
        }

        var existingUsernameUser = await _userWriteRepository.GetByUsernameAsync(
            command.Username.Trim(),
            cancellationToken);

        if (existingUsernameUser is not null)
        {
            return Result<Guid>.Failure(
                new Error(
                    "m102.user.username.exists",
                    "A user with the same username already exists.",
                    ErrorCategory.Conflict));
        }

        var identityKey = UserIdentityKey.Create(command.IdentityKey);
        var username = Username.Create(command.Username);
        var displayName = UserDisplayName.Create(command.DisplayName);
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);

        var user = User.Create(
            identityKey,
            username,
            displayName,
            command.IsActive,
            changeContext);

        await _userWriteRepository.AddAsync(user, cancellationToken);

        await _auditService.WriteAsync(
            userId: securityContext.UserId,
            username: securityContext.DisplayName,
            action: "CreateUser",
            entity: "User",
            entityId: user.Id.ToString(),
            timestampUtc: changeContext.ChangedAtUtc,
            oldValues: null,
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

        return Result<Guid>.Success(user.Id);
    }
}