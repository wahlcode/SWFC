using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Exceptions;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M200_Business.M202_Maintenance.MaintenanceOrders;

public sealed record GetMaintenanceOrderByIdQuery(Guid Id);

public sealed class GetMaintenanceOrderByIdPolicy : IAuthorizationPolicy<GetMaintenanceOrderByIdQuery>
{
    public AuthorizationRequirement GetRequirement(GetMaintenanceOrderByIdQuery request)
    {
        return new AuthorizationRequirement(
            requiredPermissions: new[] { "m202.maintenance-orders.read" });
    }
}

public sealed class GetMaintenanceOrderByIdHandler
    : IUseCaseHandler<GetMaintenanceOrderByIdQuery, MaintenanceOrderDetailsDto>
{
    private readonly IMaintenanceOrderReadRepository _readRepository;

    public GetMaintenanceOrderByIdHandler(IMaintenanceOrderReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<Result<MaintenanceOrderDetailsDto>> HandleAsync(
        GetMaintenanceOrderByIdQuery request,
        CancellationToken cancellationToken = default)
    {
        var order = await _readRepository.GetByIdAsync(request.Id, cancellationToken);

        if (order is null)
            throw new NotFoundException($"Maintenance order '{request.Id}' was not found.");

        var dto = new MaintenanceOrderDetailsDto(
            order.Id,
            order.Number.Value,
            order.Title.Value,
            order.Description.Value,
            order.Type,
            order.Status,
            order.Priority,
            order.TargetType,
            order.TargetId,
            order.MaintenancePlanId,
            order.PlannedStartUtc,
            order.PlannedEndUtc,
            order.StartedAtUtc,
            order.CompletedAtUtc,
            order.DueAtUtc,
            order.Materials
                .Select(x => new MaintenanceOrderMaterialDto(
                    x.Id,
                    x.ItemId,
                    x.Quantity,
                    x.AuditInfo.CreatedAtUtc,
                    x.AuditInfo.CreatedBy,
                    x.AuditInfo.LastModifiedAtUtc,
                    x.AuditInfo.LastModifiedBy))
                .ToList(),
            order.AuditInfo.CreatedAtUtc,
            order.AuditInfo.CreatedBy,
            order.AuditInfo.LastModifiedAtUtc,
            order.AuditInfo.LastModifiedBy);

        return Result<MaintenanceOrderDetailsDto>.Success(dto);
    }
}
