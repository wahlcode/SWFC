using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M200_Business.M202_Maintenance.MaintenancePlans;

public sealed record GetMaintenancePlansQuery;

public sealed class GetMaintenancePlansPolicy : IAuthorizationPolicy<GetMaintenancePlansQuery>
{
    public AuthorizationRequirement GetRequirement(GetMaintenancePlansQuery request)
    {
        return new AuthorizationRequirement(
            requiredPermissions: new[] { "m202.maintenance-plans.read" });
    }
}

public sealed class GetMaintenancePlansHandler
    : IUseCaseHandler<GetMaintenancePlansQuery, IReadOnlyList<MaintenancePlanListItemDto>>
{
    private readonly IMaintenancePlanReadRepository _readRepository;

    public GetMaintenancePlansHandler(IMaintenancePlanReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<Result<IReadOnlyList<MaintenancePlanListItemDto>>> HandleAsync(
        GetMaintenancePlansQuery request,
        CancellationToken cancellationToken = default)
    {
        var plans = await _readRepository.GetAllAsync(cancellationToken);

        IReadOnlyList<MaintenancePlanListItemDto> items = plans
            .OrderByDescending(x => x.AuditInfo.CreatedAtUtc)
            .Select(x => new MaintenancePlanListItemDto(
                x.Id,
                x.Name.Value,
                x.Description.Value,
                x.TargetType,
                x.TargetId,
                x.IntervalValue,
                x.IntervalUnit,
                x.NextDueAtUtc,
                x.IsActive,
                x.AuditInfo.CreatedAtUtc,
                x.AuditInfo.CreatedBy))
            .ToList();

        return Result<IReadOnlyList<MaintenancePlanListItemDto>>.Success(items);
    }
}
