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

public sealed record UpdateUserCommand(
    Guid UserId,
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
    UserType UserType,
    string Reason);

public sealed class UpdateUserValidator : ICommandValidator<UpdateUserCommand>
{
    private static readonly string[] SupportedCultures =
    [
        "de-DE",
        "en-US",
        "ru-RU",
        "it-IT",
        "sr-RS",
        "es-MX",
        "hu-HU",
        "pl-PL"
    ];

    public Task<ValidationResult> ValidateAsync(
        UpdateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.UserId == Guid.Empty)
        {
            result.Add("UserId", "UserId is required.");
        }

        Require(result, "Username", command.Username);
        Require(result, "DisplayName", command.DisplayName);
        Require(result, "FirstName", command.FirstName);
        Require(result, "LastName", command.LastName);
        Require(result, "EmployeeNumber", command.EmployeeNumber);
        Require(result, "BusinessEmail", command.BusinessEmail);
        Require(result, "BusinessPhone", command.BusinessPhone);
        Require(result, "Plant", command.Plant);
        Require(result, "Location", command.Location);
        Require(result, "Team", command.Team);
        Require(result, "JobFunction", command.JobFunction);
        Require(result, "PreferredCultureName", command.PreferredCultureName);
        Require(result, "Reason", command.Reason);

        if (!SupportedCultures.Contains(command.PreferredCultureName, StringComparer.OrdinalIgnoreCase))
        {
            result.Add("PreferredCultureName", "Culture not supported.");
        }

        return Task.FromResult(result);
    }

    private static void Require(ValidationResult result, string field, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result.Add(field, $"{field} is required.");
        }
    }
}

public sealed class UpdateUserHandler : IUseCaseHandler<UpdateUserCommand, bool>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserWriteRepository _userWriteRepository;
    private readonly IOrganizationUnitReadRepository _organizationUnitReadRepository;
    private readonly ICostCenterReadRepository _costCenterReadRepository;
    private readonly IShiftModelReadRepository _shiftModelReadRepository;
    private readonly IUserHistoryWriteRepository _userHistoryWriteRepository;
    private readonly IAuditService _auditService;

    public UpdateUserHandler(
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

        if (command.CostCenterId.HasValue)
        {
            var costCenter = await _costCenterReadRepository.GetByIdAsync(command.CostCenterId.Value, cancellationToken);
            if (costCenter is null)
            {
                return Result<bool>.Failure(
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
                return Result<bool>.Failure(
                    new Error(
                        "m102.user.shift_model.not_found",
                        "Shift model was not found.",
                        ErrorCategory.NotFound));
            }
        }

        var oldValues = UserAuditPayloads.SerializeSnapshot(user);

        var username = Username.Create(command.Username);
        var displayName = UserDisplayName.Create(command.DisplayName);
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);

        user.UpdateDetails(
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
            command.UserType,
            changeContext);

        var organizationUnits = await UserRelatedDataResolver.ResolveOrganizationUnitsAsync(
            _organizationUnitReadRepository,
            user,
            cancellationToken);

        await _userHistoryWriteRepository.AddAsync(
            UserHistoryEntry.Create(
                user.Id,
                UserHistoryChangeType.MasterDataUpdated,
                "User master data updated",
                UserHistorySnapshots.Create(user, organizationUnits),
                command.Reason,
                securityContext.UserId,
                changeContext.ChangedAtUtc),
            cancellationToken);

        var costCenterLabel = await UserRelatedDataResolver.ResolveCostCenterLabelAsync(
            _costCenterReadRepository,
            user.CostCenterId,
            cancellationToken);

        var shiftModelLabel = await UserRelatedDataResolver.ResolveShiftModelLabelAsync(
            _shiftModelReadRepository,
            user.ShiftModelId,
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
                NewValues: UserAuditPayloads.SerializeUpdated(
                    user,
                    costCenterLabel,
                    shiftModelLabel,
                    organizationUnits),
                TargetUserId: user.Id.ToString(),
                Reason: command.Reason),
            cancellationToken);

        await _userWriteRepository.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
