using SWFC.Application.M100_System.M102_Organization.CostCenters;
using SWFC.Application.M100_System.M102_Organization.OrganizationUnits;
using SWFC.Application.M100_System.M102_Organization.ShiftModels;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M100_System.M102_Organization.Users;

namespace SWFC.Application.M100_System.M102_Organization.Users;

public sealed record CreateUserCommand(
    string IdentityKey,
    string Username,
    string DisplayName,
    string FirstName,
    string LastName,
    string EmployeeNumber,
    string BusinessEmail,
    string BusinessPhone,
    string Plant,
    string Location,
    string Team,
    Guid? CostCenterId,
    Guid? ShiftModelId,
    string JobFunction,
    string PreferredCultureName,
    Guid PrimaryOrganizationUnitId,
    UserStatus Status,
    UserType UserType,
    string Reason);

public sealed class CreateUserPolicy : IAuthorizationPolicy<CreateUserCommand>
{
    public AuthorizationRequirement GetRequirement(CreateUserCommand request) =>
        new(requiredPermissions: new[] { "organization.write" });
}

public sealed class CreateUserHandler : IUseCaseHandler<CreateUserCommand, Guid>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserWriteRepository _userWriteRepository;
    private readonly IOrganizationUnitReadRepository _organizationUnitReadRepository;
    private readonly ICostCenterReadRepository _costCenterReadRepository;
    private readonly IShiftModelReadRepository _shiftModelReadRepository;
    private readonly IUserHistoryWriteRepository _userHistoryWriteRepository;
    private readonly IAuditService _auditService;

    public CreateUserHandler(
        ICurrentUserService currentUserService,
        IUserWriteRepository userWriteRepository,
        IOrganizationUnitReadRepository organizationUnitReadRepository,
        ICostCenterReadRepository costCenterReadRepository,
        IShiftModelReadRepository shiftModelReadRepository,
        IUserHistoryWriteRepository userHistoryWriteRepository,
        IAuditService auditService)
    {
        _currentUserService = currentUserService;
        _userWriteRepository = userWriteRepository;
        _organizationUnitReadRepository = organizationUnitReadRepository;
        _costCenterReadRepository = costCenterReadRepository;
        _shiftModelReadRepository = shiftModelReadRepository;
        _userHistoryWriteRepository = userHistoryWriteRepository;
        _auditService = auditService;
    }

    public async Task<Result<Guid>> HandleAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);

        var primaryOrganizationUnit = await _organizationUnitReadRepository.GetByIdAsync(
            command.PrimaryOrganizationUnitId,
            cancellationToken);

        if (primaryOrganizationUnit is null)
        {
            return Result<Guid>.Failure(
                new Error(
                    "m102.user.primary_organization.not_found",
                    "Primary organization unit was not found.",
                    ErrorCategory.NotFound));
        }

        if (command.CostCenterId.HasValue)
        {
            var costCenter = await _costCenterReadRepository.GetByIdAsync(command.CostCenterId.Value, cancellationToken);
            if (costCenter is null)
            {
                return Result<Guid>.Failure(
                    new Error(
                        "m102.user.cost_center.not_found",
                        "Cost center was not found.",
                        ErrorCategory.NotFound));
            }
        }

        if (command.ShiftModelId.HasValue)
        {
            var shiftModel = await _shiftModelReadRepository.GetByIdAsync(command.ShiftModelId.Value, cancellationToken);
            if (shiftModel is null)
            {
                return Result<Guid>.Failure(
                    new Error(
                        "m102.user.shift_model.not_found",
                        "Shift model was not found.",
                        ErrorCategory.NotFound));
            }
        }

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
            command.FirstName,
            command.LastName,
            command.EmployeeNumber,
            command.BusinessEmail,
            command.BusinessPhone,
            command.Plant,
            command.Location,
            command.Team,
            command.CostCenterId,
            command.ShiftModelId,
            command.JobFunction,
            command.PreferredCultureName,
            command.Status,
            command.UserType,
            changeContext);

        user.AssignOrganizationUnit(primaryOrganizationUnit.Id, isPrimary: true, changeContext);

        await _userWriteRepository.AddAsync(user, cancellationToken);

        var organizationUnitLabels = new[]
        {
            $"{primaryOrganizationUnit.Name.Value} ({primaryOrganizationUnit.Code.Value}) [Primary]"
        };

        await _userHistoryWriteRepository.AddAsync(
            UserHistoryEntry.Create(
                user.Id,
                UserHistoryChangeType.Created,
                "User created",
                UserHistorySnapshots.Create(user, organizationUnitLabels),
                command.Reason,
                securityContext.UserId,
                changeContext.ChangedAtUtc),
            cancellationToken);

        var costCenterLabel = await UserRelatedDataResolver.ResolveCostCenterLabelAsync(
            _costCenterReadRepository,
            command.CostCenterId,
            cancellationToken);

        var shiftModelLabel = await UserRelatedDataResolver.ResolveShiftModelLabelAsync(
            _shiftModelReadRepository,
            command.ShiftModelId,
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
                NewValues: UserAuditPayloads.SerializeCreated(
                    user,
                    costCenterLabel,
                    shiftModelLabel,
                    primaryOrganizationUnit.Id,
                    primaryOrganizationUnit.Name.Value,
                    primaryOrganizationUnit.Code.Value),
                TargetUserId: user.Id.ToString(),
                Reason: command.Reason),
            cancellationToken);

        await _userWriteRepository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(user.Id);
    }
}
