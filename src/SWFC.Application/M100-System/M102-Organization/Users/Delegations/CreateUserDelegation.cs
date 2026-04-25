using System.Text.Json;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M100_System.M102_Organization.Users.Delegations;

namespace SWFC.Application.M100_System.M102_Organization.Users.Delegations;

public sealed record CreateUserDelegationCommand(
    Guid UserId,
    Guid DelegateUserId,
    string DelegationType,
    DateTime ValidFromUtc,
    DateTime? ValidToUtc,
    string Reason);

public sealed class CreateUserDelegationValidator : ICommandValidator<CreateUserDelegationCommand>
{
    public Task<ValidationResult> ValidateAsync(
        CreateUserDelegationCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.UserId == Guid.Empty)
            result.Add("UserId", "User id is required.");

        if (command.DelegateUserId == Guid.Empty)
            result.Add("DelegateUserId", "Delegate user id is required.");

        if (command.UserId != Guid.Empty &&
            command.DelegateUserId != Guid.Empty &&
            command.UserId == command.DelegateUserId)
        {
            result.Add("DelegateUserId", "Delegate user must be different from user.");
        }

        if (string.IsNullOrWhiteSpace(command.DelegationType))
            result.Add("DelegationType", "Delegation type is required.");

        if (string.IsNullOrWhiteSpace(command.Reason))
            result.Add("Reason", "Reason is required.");

        if (command.ValidToUtc.HasValue && command.ValidToUtc.Value < command.ValidFromUtc)
            result.Add("ValidToUtc", "Valid to must be greater than or equal to valid from.");

        return Task.FromResult(result);
    }
}

public sealed class CreateUserDelegationPolicy : IAuthorizationPolicy<CreateUserDelegationCommand>
{
    public AuthorizationRequirement GetRequirement(CreateUserDelegationCommand request) =>
        new(requiredPermissions: new[] { "organization.users.write" });
}

public sealed class CreateUserDelegationHandler : IUseCaseHandler<CreateUserDelegationCommand, Guid>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserDelegationWriteRepository _userDelegationWriteRepository;
    private readonly IAuditService _auditService;

    public CreateUserDelegationHandler(
        ICurrentUserService currentUserService,
        IUserDelegationWriteRepository userDelegationWriteRepository,
        IAuditService auditService)
    {
        _currentUserService = currentUserService;
        _userDelegationWriteRepository = userDelegationWriteRepository;
        _auditService = auditService;
    }

    public async Task<Result<Guid>> HandleAsync(
        CreateUserDelegationCommand command,
        CancellationToken cancellationToken = default)
    {
        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);

        var delegation = UserDelegation.Create(
            command.UserId,
            command.DelegateUserId,
            command.DelegationType,
            command.ValidFromUtc,
            command.ValidToUtc,
            changeContext);

        await _userDelegationWriteRepository.AddAsync(delegation, cancellationToken);

        await _auditService.WriteAsync(
            new AuditWriteRequest(
                ActorUserId: securityContext.UserId,
                ActorDisplayName: securityContext.DisplayName,
                Action: "UserDelegationChanged",
                Module: "M102",
                ObjectType: "UserDelegation",
                ObjectId: delegation.Id.ToString(),
                TimestampUtc: changeContext.ChangedAtUtc,
                NewValues: JsonSerializer.Serialize(new
                {
                    ChangeType = "Created",
                    delegation.Id,
                    delegation.UserId,
                    delegation.DelegateUserId,
                    delegation.DelegationType,
                    delegation.ValidFromUtc,
                    delegation.ValidToUtc,
                    delegation.IsActive
                }),
                TargetUserId: delegation.UserId.ToString(),
                Reason: command.Reason),
            cancellationToken);

        await _userDelegationWriteRepository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(delegation.Id);
    }
}