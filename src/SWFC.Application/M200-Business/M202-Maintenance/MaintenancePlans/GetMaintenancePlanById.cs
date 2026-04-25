using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Exceptions;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M200_Business.M202_Maintenance.MaintenancePlans;

public sealed record GetMaintenancePlanByIdQuery(Guid Id);

public sealed class GetMaintenancePlanByIdPolicy : IAuthorizationPolicy<GetMaintenancePlanByIdQuery>
{
    public AuthorizationRequirement GetRequirement(GetMaintenancePlanByIdQuery request)
    {
        return new AuthorizationRequirement(
            requiredPermissions: new[] { "m202.maintenance-plans.read" });
    }
}

public sealed class GetMaintenancePlanByIdHandler
    : IUseCaseHandler<GetMaintenancePlanByIdQuery, MaintenancePlanDetailsDto>
{
    private readonly IMaintenancePlanReadRepository _readRepository;

    public GetMaintenancePlanByIdHandler(IMaintenancePlanReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<Result<MaintenancePlanDetailsDto>> HandleAsync(
        GetMaintenancePlanByIdQuery request,
        CancellationToken cancellationToken = default)
    {
        var plan = await _readRepository.GetByIdAsync(request.Id, cancellationToken);

        if (plan is null)
            throw new NotFoundException($"Maintenance plan '{request.Id}' was not found.");

        var dto = new MaintenancePlanDetailsDto(
            plan.Id,
            plan.Name.Value,
            plan.Description.Value,
            plan.TargetType,
            plan.TargetId,
            plan.IntervalValue,
            plan.IntervalUnit,
            plan.NextDueAtUtc,
            plan.IsActive,
            plan.AuditInfo.CreatedAtUtc,
            plan.AuditInfo.CreatedBy,
            plan.AuditInfo.LastModifiedAtUtc,
            plan.AuditInfo.LastModifiedBy);

        return Result<MaintenancePlanDetailsDto>.Success(dto);
    }
}
