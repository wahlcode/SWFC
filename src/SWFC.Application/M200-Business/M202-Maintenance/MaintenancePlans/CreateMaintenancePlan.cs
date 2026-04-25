using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M200_Business.M202_Maintenance.MaintenanceOrders;
using SWFC.Domain.M200_Business.M202_Maintenance.MaintenancePlans;

namespace SWFC.Application.M200_Business.M202_Maintenance.MaintenancePlans;

public sealed record CreateMaintenancePlanCommand(
    string Name,
    string Description,
    MaintenanceTargetType TargetType,
    Guid TargetId,
    int IntervalValue,
    MaintenancePlanIntervalUnit IntervalUnit,
    DateTime NextDueAtUtc);

public sealed class CreateMaintenancePlanValidator : ICommandValidator<CreateMaintenancePlanCommand>
{
    public Task<ValidationResult> ValidateAsync(
        CreateMaintenancePlanCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (string.IsNullOrWhiteSpace(command.Name))
            result.Add("M202.Plan.Name.Required", "Name is required.");

        if (string.IsNullOrWhiteSpace(command.Description))
            result.Add("M202.Plan.Description.Required", "Description is required.");

        if (command.TargetId == Guid.Empty)
            result.Add("M202.Plan.Target.Required", "Target id is required.");

        if (command.IntervalValue <= 0)
            result.Add("M202.Plan.Interval.Invalid", "Interval must be greater than zero.");

        return Task.FromResult(result);
    }
}

public sealed class CreateMaintenancePlanPolicy : IAuthorizationPolicy<CreateMaintenancePlanCommand>
{
    public AuthorizationRequirement GetRequirement(CreateMaintenancePlanCommand request)
    {
        return new AuthorizationRequirement(
            requiredPermissions: new[] { "m202.maintenance-plans.create" });
    }
}

public sealed class CreateMaintenancePlanHandler : IUseCaseHandler<CreateMaintenancePlanCommand, Guid>
{
    private readonly IMaintenancePlanWriteRepository _writeRepository;

    public CreateMaintenancePlanHandler(IMaintenancePlanWriteRepository writeRepository)
    {
        _writeRepository = writeRepository;
    }

    public async Task<Result<Guid>> HandleAsync(
        CreateMaintenancePlanCommand request,
        CancellationToken cancellationToken = default)
    {
        var changeContext = ChangeContext.Create("system", "Create maintenance plan");

        var plan = MaintenancePlan.Create(
            new MaintenancePlanName(request.Name),
            new MaintenancePlanDescription(request.Description),
            request.TargetType,
            request.TargetId,
            request.IntervalValue,
            request.IntervalUnit,
            request.NextDueAtUtc,
            changeContext);

        await _writeRepository.AddAsync(plan, cancellationToken);
        await _writeRepository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(plan.Id);
    }
}
