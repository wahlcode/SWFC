using SWFC.Application.M100_System.M102_Organization.OrganizationUnits;
using SWFC.Application.M100_System.M102_Organization.Users;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;

namespace SWFC.Application.M100_System.M102_Organization.Assignments;

public sealed record RemoveOrganizationUnitFromUserCommand(
    Guid UserId,
    Guid OrganizationUnitId,
    string Reason);

public sealed class RemoveOrganizationUnitFromUserValidator : ICommandValidator<RemoveOrganizationUnitFromUserCommand>
{
    public Task<ValidationResult> ValidateAsync(
        RemoveOrganizationUnitFromUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.UserId == Guid.Empty)
        {
            result.Add("UserId", "User id is required.");
        }

        if (command.OrganizationUnitId == Guid.Empty)
        {
            result.Add("OrganizationUnitId", "Organization unit id is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            result.Add("Reason", "Reason is required.");
        }

        return Task.FromResult(result);
    }
}

public sealed class RemoveOrganizationUnitFromUserPolicy : IAuthorizationPolicy<RemoveOrganizationUnitFromUserCommand>
{
    public AuthorizationRequirement GetRequirement(RemoveOrganizationUnitFromUserCommand request) =>
        new(requiredPermissions: new[] { "organization.write" });
}

public sealed class RemoveOrganizationUnitFromUserHandler : IUseCaseHandler<RemoveOrganizationUnitFromUserCommand, bool>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserOrganizationAssignmentWriteRepository _userOrganizationAssignmentWriteRepository;
    private readonly IUserReadRepository _userReadRepository;
    private readonly IOrganizationUnitReadRepository _organizationUnitReadRepository;
    private readonly IUserHistoryWriteRepository _userHistoryWriteRepository;
    private readonly IAuditService _auditService;

    public RemoveOrganizationUnitFromUserHandler(
        ICurrentUserService currentUserService,
        IUserOrganizationAssignmentWriteRepository userOrganizationAssignmentWriteRepository,
        IUserReadRepository userReadRepository,
        IOrganizationUnitReadRepository organizationUnitReadRepository,
        IUserHistoryWriteRepository userHistoryWriteRepository,
        IAuditService auditService)
    {
        _currentUserService = currentUserService;
        _userOrganizationAssignmentWriteRepository = userOrganizationAssignmentWriteRepository;
        _userReadRepository = userReadRepository;
        _organizationUnitReadRepository = organizationUnitReadRepository;
        _userHistoryWriteRepository = userHistoryWriteRepository;
        _auditService = auditService;
    }

    public async Task<Result<bool>> HandleAsync(
        RemoveOrganizationUnitFromUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);
        var userBeforeChange = await _userReadRepository.GetByIdAsync(command.UserId, cancellationToken);

        if (userBeforeChange is null)
        {
            return Result<bool>.Failure(
                new Error(
                    "m102.assignment.remove_organization_unit.user_not_found",
                    "User was not found.",
                    ErrorCategory.NotFound));
        }

        var assignmentToRemove = userBeforeChange.OrganizationUnits
            .FirstOrDefault(x => x.OrganizationUnitId == command.OrganizationUnitId && x.IsActive);

        if (assignmentToRemove is null)
        {
            return Result<bool>.Failure(
                new Error(
                    "m102.assignment.remove_organization_unit.assignment_not_found",
                    "Organization assignment was not found.",
                    ErrorCategory.NotFound));
        }

        var organizationUnit = await _organizationUnitReadRepository.GetByIdAsync(command.OrganizationUnitId, cancellationToken);

        if (organizationUnit is null)
        {
            return Result<bool>.Failure(
                new Error(
                    "m102.assignment.remove_organization_unit.organization_not_found",
                    "Organization unit was not found.",
                    ErrorCategory.NotFound));
        }

        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);

        var changed = await _userOrganizationAssignmentWriteRepository.RemoveOrganizationUnitAsync(
            command.UserId,
            command.OrganizationUnitId,
            changeContext,
            cancellationToken);

        if (!changed)
        {
            return Result<bool>.Failure(
                new Error(
                    "m102.assignment.remove_organization_unit.failed",
                    "Organization unit could not be removed from user.",
                    ErrorCategory.Validation));
        }

        var userAfterChange = await _userReadRepository.GetByIdAsync(command.UserId, cancellationToken);

        if (userAfterChange is null)
        {
            return Result<bool>.Failure(
                new Error(
                    "m102.assignment.remove_organization_unit.user_reload_failed",
                    "User could not be reloaded after organization removal.",
                    ErrorCategory.Technical));
        }

        var organizationUnitsAfterChange = await ResolveOrganizationUnitsAsync(userAfterChange, cancellationToken);

        await _userHistoryWriteRepository.AddAsync(
            Domain.M100_System.M102_Organization.Users.UserHistoryEntry.Create(
                userAfterChange.Id,
                Domain.M100_System.M102_Organization.Users.UserHistoryChangeType.SecondaryOrganizationRemoved,
                $"Organization removed: {organizationUnit.Name.Value} ({organizationUnit.Code.Value})",
                UserHistorySnapshots.Create(userAfterChange, organizationUnitsAfterChange),
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
                ObjectType: "UserOrganizationAssignment",
                ObjectId: $"{command.UserId:N}:{command.OrganizationUnitId:N}",
                TimestampUtc: changeContext.ChangedAtUtc,
                OldValues: UserHistorySnapshots.Create(userBeforeChange, await ResolveOrganizationUnitsAsync(userBeforeChange, cancellationToken)),
                NewValues: UserHistorySnapshots.Create(userAfterChange, organizationUnitsAfterChange),
                TargetUserId: command.UserId.ToString(),
                Reason: command.Reason),
            cancellationToken);

        return Result<bool>.Success(true);
    }

    private async Task<IReadOnlyCollection<string>> ResolveOrganizationUnitsAsync(
        Domain.M100_System.M102_Organization.Users.User user,
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

