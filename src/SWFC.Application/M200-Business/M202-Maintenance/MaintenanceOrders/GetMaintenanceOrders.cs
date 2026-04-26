using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M200_Business.M202_Maintenance.MaintenanceOrders;

public sealed record GetMaintenanceOrdersQuery;

public sealed class GetMaintenanceOrdersPolicy : IAuthorizationPolicy<GetMaintenanceOrdersQuery>
{
    public AuthorizationRequirement GetRequirement(GetMaintenanceOrdersQuery request)
    {
        return new AuthorizationRequirement(
            requiredPermissions: new[] { "m202.maintenance-orders.read" });
    }
}

public sealed class GetMaintenanceOrdersHandler
    : IUseCaseHandler<GetMaintenanceOrdersQuery, IReadOnlyList<MaintenanceOrderListItemDto>>
{
    private readonly IMaintenanceOrderReadRepository _readRepository;

    public GetMaintenanceOrdersHandler(IMaintenanceOrderReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<Result<IReadOnlyList<MaintenanceOrderListItemDto>>> HandleAsync(
        GetMaintenanceOrdersQuery request,
        CancellationToken cancellationToken = default)
    {
        var orders = await _readRepository.GetAllAsync(cancellationToken);

        IReadOnlyList<MaintenanceOrderListItemDto> items = orders
            .OrderByDescending(x => x.AuditInfo.CreatedAtUtc)
            .Select(x => new MaintenanceOrderListItemDto(
                x.Id,
                x.Number.Value,
                x.Title.Value,
                x.Type,
                x.Status,
                x.Priority,
                x.TargetType,
                x.TargetId,
                x.MaintenancePlanId,
                x.PlannedStartUtc,
                x.PlannedEndUtc,
                x.StartedAtUtc,
                x.CompletedAtUtc,
                x.DueAtUtc,
                x.AuditInfo.CreatedAtUtc,
                x.AuditInfo.CreatedBy,
                x.AuditInfo.LastModifiedAtUtc,
                x.AuditInfo.LastModifiedBy))
            .ToList();

        return Result<IReadOnlyList<MaintenanceOrderListItemDto>>.Success(items);
    }
}
