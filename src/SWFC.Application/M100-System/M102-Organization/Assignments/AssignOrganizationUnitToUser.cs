using System.Text.Json;
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

public sealed record AssignOrganizationUnitToUserCommand(
    Guid UserId,
    Guid OrganizationUnitId,
    bool IsPrimary,
    string Reason);

public sealed class AssignOrganizationUnitToUserValidator : ICommandValidator<AssignOrganizationUnitToUserCommand>
{
    public Task<ValidationResult> ValidateAsync(
        AssignOrganizationUnitToUserCommand command,
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

public sealed class AssignOrganizationUnitToUserPolicy : IAuthorizationPolicy<AssignOrganizationUnitToUserCommand>
{
    public AuthorizationRequirement GetRequirement(AssignOrganizationUnitToUserCommand request) =>
        new(requiredPermissions: new[] { "organization.write" });
}

public sealed class AssignOrganizationUnitToUserHandler : IUseCaseHandler<AssignOrganizationUnitToUserCommand, bool>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserOrganizationAssignmentWriteRepository _userOrganizationAssignmentWriteRepository;
    private readonly IUserReadRepository _userReadRepository;
    private readonly IOrganizationUnitReadRepository _organizationUnitReadRepository;
    private readonly IUserHistoryWriteRepository _userHistoryWriteRepository;
    private readonly IAuditService _auditService;

    public AssignOrganizationUnitToUserHandler(
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
        AssignOrganizationUnitToUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);
        var userBeforeChange = await _userReadRepository.GetByIdAsync(command.UserId, cancellationToken);
        var organizationUnit = await _organizationUnitReadRepository.GetByIdAsync(command.OrganizationUnitId, cancellationToken);

        if (userBeforeChange is null || organizationUnit is null)
        {
            return Result<bool>.Failure(
                new Error(
                    "m102.assignment.assign_organization_unit.not_found",
                    "User or organization unit was not found.",
                    ErrorCategory.NotFound));
        }

        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);

        var changed = await _userOrganizationAssignmentWriteRepository.AssignOrganizationUnitAsync(
            command.UserId,
            command.OrganizationUnitId,
            command.IsPrimary,
            changeContext,
            cancellationToken);

        if (!changed)
        {
            return Result<bool>.Failure(
                new Error(
                    "m102.assignment.assign_organization_unit.failed",
                    "Organization unit could not be assigned to user.",
                    ErrorCategory.Validation));
        }

        var userAfterChange = await _userReadRepository.GetByIdAsync(command.UserId, cancellationToken);

        if (userAfterChange is null)
        {
            return Result<bool>.Failure(
                new Error(
                    "m102.assignment.assign_organization_unit.user_reload_failed",
                    "User could not be reloaded after organization assignment.",
                    ErrorCategory.Technical));
        }

        var organizationUnits = await ResolveOrganizationUnitsAsync(userAfterChange, cancellationToken);
        var changeType = command.IsPrimary
            ? Domain.M100_System.M102_Organization.Users.UserHistoryChangeType.PrimaryOrganizationAssigned
            : Domain.M100_System.M102_Organization.Users.UserHistoryChangeType.SecondaryOrganizationAdded;
        var summary = command.IsPrimary
            ? $"Primary organization assigned: {organizationUnit.Name.Value} ({organizationUnit.Code.Value})"
            : $"Secondary organization added: {organizationUnit.Name.Value} ({organizationUnit.Code.Value})";

        await _userHistoryWriteRepository.AddAsync(
            Domain.M100_System.M102_Organization.Users.UserHistoryEntry.Create(
                userAfterChange.Id,
                changeType,
                summary,
                UserHistorySnapshots.Create(userAfterChange, organizationUnits),
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
                NewValues: UserHistorySnapshots.Create(userAfterChange, organizationUnits),
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

