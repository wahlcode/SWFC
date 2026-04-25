using System.Text.Json;
using SWFC.Application.M100_System.M102_Organization.OrganizationUnits;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M100_System.M102_Organization.Users;

namespace SWFC.Application.M100_System.M102_Organization.Users;

public sealed record ChangeUserStatusCommand(
    Guid UserId,
    UserStatus Status,
    string Reason);

public sealed class ChangeUserStatusValidator : ICommandValidator<ChangeUserStatusCommand>
{
    public Task<ValidationResult> ValidateAsync(
        ChangeUserStatusCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.UserId == Guid.Empty)
        {
            result.Add("UserId", "User id is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            result.Add("Reason", "Reason is required.");
        }

        return Task.FromResult(result);
    }
}

public sealed class ChangeUserStatusPolicy : IAuthorizationPolicy<ChangeUserStatusCommand>
{
    public AuthorizationRequirement GetRequirement(ChangeUserStatusCommand request) =>
        new(requiredPermissions: new[] { "organization.write" });
}

public sealed class ChangeUserStatusHandler : IUseCaseHandler<ChangeUserStatusCommand, bool>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserWriteRepository _userWriteRepository;
    private readonly IOrganizationUnitReadRepository _organizationUnitReadRepository;
    private readonly IUserHistoryWriteRepository _userHistoryWriteRepository;
    private readonly IAuditService _auditService;

    public ChangeUserStatusHandler(
        ICurrentUserService currentUserService,
        IUserWriteRepository userWriteRepository,
        IOrganizationUnitReadRepository organizationUnitReadRepository,
        IUserHistoryWriteRepository userHistoryWriteRepository,
        IAuditService auditService)
    {
        _currentUserService = currentUserService;
        _userWriteRepository = userWriteRepository;
        _organizationUnitReadRepository = organizationUnitReadRepository;
        _userHistoryWriteRepository = userHistoryWriteRepository;
        _auditService = auditService;
    }

    public async Task<Result<bool>> HandleAsync(
        ChangeUserStatusCommand command,
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

        var oldValues = JsonSerializer.Serialize(new
        {
            user.Id,
            Username = user.Username.Value,
            DisplayName = user.DisplayName.Value,
            Status = user.Status.ToString(),
            user.IsActive
        });

        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);
        user.ChangeStatus(command.Status, changeContext);

        var organizationUnits = await ResolveOrganizationUnitsAsync(user, cancellationToken);

        await _userHistoryWriteRepository.AddAsync(
            UserHistoryEntry.Create(
                user.Id,
                UserHistoryChangeType.StatusChanged,
                $"User status changed to {user.Status}",
                UserHistorySnapshots.Create(user, organizationUnits),
                command.Reason,
                securityContext.UserId,
                changeContext.ChangedAtUtc),
            cancellationToken);

        await _auditService.WriteAsync(
            new AuditWriteRequest(
                ActorUserId: securityContext.UserId,
                ActorDisplayName: securityContext.DisplayName,
                Action: "UserChanged",
                Module: "M102",
                ObjectType: "User",
                ObjectId: user.Id.ToString(),
                TimestampUtc: changeContext.ChangedAtUtc,
                OldValues: oldValues,
                NewValues: JsonSerializer.Serialize(new
                {
                    user.Id,
                    Username = user.Username.Value,
                    DisplayName = user.DisplayName.Value,
                    Status = user.Status.ToString(),
                    user.IsActive,
                    OrganizationUnits = organizationUnits
                }),
                TargetUserId: user.Id.ToString(),
                Reason: command.Reason),
            cancellationToken);

        await _userWriteRepository.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }

    private async Task<IReadOnlyCollection<string>> ResolveOrganizationUnitsAsync(
        User user,
        CancellationToken cancellationToken)
    {
        var items = new List<string>();

        foreach (var assignment in user.OrganizationUnits.Where(x => x.IsActive))
        {
            var organizationUnit = await _organizationUnitReadRepository.GetByIdAsync(
                assignment.OrganizationUnitId,
                cancellationToken);

            if (organizationUnit is null)
            {
                continue;
            }

            items.Add($"{organizationUnit.Name.Value} ({organizationUnit.Code.Value}){(assignment.IsPrimary ? " [Primary]" : string.Empty)}");
        }

        return items;
    }
}
