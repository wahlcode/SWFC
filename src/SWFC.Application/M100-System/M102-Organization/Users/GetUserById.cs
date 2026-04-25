using SWFC.Application.M100_System.M102_Organization.CostCenters;
using SWFC.Application.M100_System.M102_Organization.OrganizationUnits;
using SWFC.Application.M100_System.M102_Organization.ShiftModels;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M100_System.M102_Organization.Users;

public sealed record GetUserByIdQuery(Guid UserId);

public sealed class GetUserByIdValidator : ICommandValidator<GetUserByIdQuery>
{
    public Task<ValidationResult> ValidateAsync(
        GetUserByIdQuery command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.UserId == Guid.Empty)
        {
            result.Add("UserId", "User id is required.");
        }

        return Task.FromResult(result);
    }
}

public sealed class GetUserByIdPolicy : IAuthorizationPolicy<GetUserByIdQuery>
{
    public AuthorizationRequirement GetRequirement(GetUserByIdQuery request) =>
        new(requiredPermissions: new[] { "organization.read" });
}

public sealed class GetUserByIdHandler : IUseCaseHandler<GetUserByIdQuery, UserDetailsDto>
{
    private readonly IUserReadRepository _userReadRepository;
    private readonly IOrganizationUnitReadRepository _organizationUnitReadRepository;
    private readonly ICostCenterReadRepository _costCenterReadRepository;
    private readonly IShiftModelReadRepository _shiftModelReadRepository;
    private readonly IUserHistoryReadRepository _userHistoryReadRepository;

    public GetUserByIdHandler(
        IUserReadRepository userReadRepository,
        IOrganizationUnitReadRepository organizationUnitReadRepository,
        ICostCenterReadRepository costCenterReadRepository,
        IShiftModelReadRepository shiftModelReadRepository,
        IUserHistoryReadRepository userHistoryReadRepository)
    {
        _userReadRepository = userReadRepository;
        _organizationUnitReadRepository = organizationUnitReadRepository;
        _costCenterReadRepository = costCenterReadRepository;
        _shiftModelReadRepository = shiftModelReadRepository;
        _userHistoryReadRepository = userHistoryReadRepository;
    }

    public async Task<Result<UserDetailsDto>> HandleAsync(
        GetUserByIdQuery command,
        CancellationToken cancellationToken = default)
    {
        var user = await _userReadRepository.GetByIdAsync(command.UserId, cancellationToken);

        if (user is null)
        {
            return Result<UserDetailsDto>.Failure(
                new Error(
                    "m102.user.not_found",
                    "User was not found.",
                    ErrorCategory.NotFound));
        }

        var resolvedOrganizationUnits = new List<OrganizationUnitReference>();

        foreach (var assignment in user.OrganizationUnits)
        {
            var organizationUnit = await _organizationUnitReadRepository.GetByIdAsync(
                assignment.OrganizationUnitId,
                cancellationToken);

            if (organizationUnit is not null)
            {
                resolvedOrganizationUnits.Add(new OrganizationUnitReference(
                    organizationUnit.Id,
                    organizationUnit.Name.Value,
                    organizationUnit.Code.Value,
                    assignment.IsPrimary));
            }
        }

        UserReferenceDto? costCenterDto = null;
        if (user.CostCenterId.HasValue)
        {
            var costCenter = await _costCenterReadRepository.GetByIdAsync(user.CostCenterId.Value, cancellationToken);
            if (costCenter is not null)
            {
                costCenterDto = new UserReferenceDto(costCenter.Id, costCenter.Name.Value, costCenter.Code.Value);
            }
        }

        UserReferenceDto? shiftModelDto = null;
        if (user.ShiftModelId.HasValue)
        {
            var shiftModel = await _shiftModelReadRepository.GetByIdAsync(user.ShiftModelId.Value, cancellationToken);
            if (shiftModel is not null)
            {
                shiftModelDto = new UserReferenceDto(shiftModel.Id, shiftModel.Name.Value, shiftModel.Code.Value);
            }
        }

        var historyEntries = await _userHistoryReadRepository.GetByUserIdAsync(user.Id, cancellationToken);

        var dto = new UserDetailsDto(
            user.Id,
            user.IdentityKey.Value,
            user.Username.Value,
            user.DisplayName.Value,
            user.FirstName,
            user.LastName,
            user.EmployeeNumber,
            user.BusinessEmail,
            user.BusinessPhone,
            user.Plant,
            user.Location,
            user.Team,
            user.CostCenterId,
            costCenterDto,
            user.ShiftModelId,
            shiftModelDto,
            user.JobFunction,
            user.PreferredCultureName,
            user.Status,
            user.UserType,
            user.IsActive,
            resolvedOrganizationUnits
                .OrderByDescending(x => x.IsPrimary)
                .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            historyEntries
                .OrderByDescending(x => x.ChangedAtUtc)
                .Select(x => new UserHistoryListItemDto(
                    x.Id,
                    x.ChangeType,
                    x.Summary,
                    x.Reason,
                    x.ChangedAtUtc,
                    x.ChangedByUserId))
                .ToArray());

        return Result<UserDetailsDto>.Success(dto);
    }
}
