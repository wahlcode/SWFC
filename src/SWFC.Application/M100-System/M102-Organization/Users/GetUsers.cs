using SWFC.Application.M100_System.M102_Organization.CostCenters;
using SWFC.Application.M100_System.M102_Organization.ShiftModels;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M100_System.M102_Organization.Users;

public sealed record GetUsersQuery;

public sealed class GetUsersPolicy : IAuthorizationPolicy<GetUsersQuery>
{
    public AuthorizationRequirement GetRequirement(GetUsersQuery request) =>
        new(requiredPermissions: new[] { "organization.read" });
}

public sealed class GetUsersHandler : IUseCaseHandler<GetUsersQuery, IReadOnlyList<UserListItem>>
{
    private readonly IUserReadRepository _userReadRepository;
    private readonly ICostCenterReadRepository _costCenterReadRepository;
    private readonly IShiftModelReadRepository _shiftModelReadRepository;

    public GetUsersHandler(
        IUserReadRepository userReadRepository,
        ICostCenterReadRepository costCenterReadRepository,
        IShiftModelReadRepository shiftModelReadRepository)
    {
        _userReadRepository = userReadRepository;
        _costCenterReadRepository = costCenterReadRepository;
        _shiftModelReadRepository = shiftModelReadRepository;
    }

    public async Task<Result<IReadOnlyList<UserListItem>>> HandleAsync(
        GetUsersQuery command,
        CancellationToken cancellationToken = default)
    {
        var users = await _userReadRepository.GetAllAsync(cancellationToken);
        var costCenters = await _costCenterReadRepository.GetAllAsync(cancellationToken);
        var shiftModels = await _shiftModelReadRepository.GetAllAsync(cancellationToken);

        var costCenterMap = costCenters.ToDictionary(x => x.Id);
        var shiftModelMap = shiftModels.ToDictionary(x => x.Id);

        var items = users
            .Select(x =>
            {
                string? costCenterName = null;
                string? costCenterCode = null;
                if (x.CostCenterId.HasValue && costCenterMap.TryGetValue(x.CostCenterId.Value, out var costCenter))
                {
                    costCenterName = costCenter.Name.Value;
                    costCenterCode = costCenter.Code.Value;
                }

                string? shiftModelName = null;
                string? shiftModelCode = null;
                if (x.ShiftModelId.HasValue && shiftModelMap.TryGetValue(x.ShiftModelId.Value, out var shiftModel))
                {
                    shiftModelName = shiftModel.Name.Value;
                    shiftModelCode = shiftModel.Code.Value;
                }

                return new UserListItem(
                    x.Id,
                    x.IdentityKey.Value,
                    x.Username.Value,
                    x.DisplayName.Value,
                    x.EmployeeNumber,
                    x.BusinessEmail,
                    x.PreferredCultureName,
                    x.CostCenterId,
                    costCenterName,
                    costCenterCode,
                    x.ShiftModelId,
                    shiftModelName,
                    shiftModelCode,
                    x.Status,
                    x.UserType,
                    x.IsActive);
            })
            .ToList();

        return Result<IReadOnlyList<UserListItem>>.Success(items);
    }
}
