using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M100_System.M102_Organization.CostCenters;

public sealed record GetCostCentersQuery;

public sealed class GetCostCentersPolicy : IAuthorizationPolicy<GetCostCentersQuery>
{
    public AuthorizationRequirement GetRequirement(GetCostCentersQuery request) =>
        new(requiredPermissions: new[] { "organization.read" });
}

public sealed class GetCostCentersHandler : IUseCaseHandler<GetCostCentersQuery, IReadOnlyList<CostCenterListItem>>
{
    private readonly ICostCenterReadRepository _costCenterReadRepository;

    public GetCostCentersHandler(ICostCenterReadRepository costCenterReadRepository)
    {
        _costCenterReadRepository = costCenterReadRepository;
    }

    public async Task<Result<IReadOnlyList<CostCenterListItem>>> HandleAsync(
        GetCostCentersQuery command,
        CancellationToken cancellationToken = default)
    {
        var costCenters = await _costCenterReadRepository.GetAllAsync(cancellationToken);
        var parentMap = costCenters.ToDictionary(x => x.Id);

        var items = costCenters
            .Select(x =>
            {
                string? parentName = null;
                string? parentCode = null;

                if (x.ParentCostCenterId.HasValue &&
                    parentMap.TryGetValue(x.ParentCostCenterId.Value, out var parent))
                {
                    parentName = parent.Name.Value;
                    parentCode = parent.Code.Value;
                }

                return new CostCenterListItem(
                    x.Id,
                    x.Name.Value,
                    x.Code.Value,
                    x.ParentCostCenterId,
                    parentName,
                    parentCode,
                    x.IsActive);
            })
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Result<IReadOnlyList<CostCenterListItem>>.Success(items);
    }
}
