using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Exceptions;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M200_Business.M202_Maintenance.MaintenanceOrders;
using SWFC.Domain.M200_Business.M202_Maintenance.MaintenancePlans;

namespace SWFC.Application.M200_Business.M202_Maintenance.MaintenancePlans;

public sealed record UpdateMaintenancePlanCommand(
    Guid Id,
    string Name,
    string Description,
    MaintenanceTargetType TargetType,
    Guid TargetId,
    int IntervalValue,
    MaintenancePlanIntervalUnit IntervalUnit,
    DateTime NextDueAtUtc,
    bool IsActive);

public sealed class UpdateMaintenancePlanValidator : ICommandValidator<UpdateMaintenancePlanCommand>
{
    public Task<ValidationResult> ValidateAsync(
        UpdateMaintenancePlanCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.Id == Guid.Empty)
            result.Add("M202.Plan.Id.Required", "Id is required.");

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

public sealed class UpdateMaintenancePlanPolicy : IAuthorizationPolicy<UpdateMaintenancePlanCommand>
{
    public AuthorizationRequirement GetRequirement(UpdateMaintenancePlanCommand request)
    {
        return new AuthorizationRequirement(
            requiredPermissions: new[] { "m202.maintenance-plans.update" });
    }
}

public sealed class UpdateMaintenancePlanHandler : IUseCaseHandler<UpdateMaintenancePlanCommand, bool>
{
    private readonly IMaintenancePlanReadRepository _readRepository;
    private readonly IMaintenancePlanWriteRepository _writeRepository;

    public UpdateMaintenancePlanHandler(
        IMaintenancePlanReadRepository readRepository,
        IMaintenancePlanWriteRepository writeRepository)
    {
        _readRepository = readRepository;
        _writeRepository = writeRepository;
    }

    public async Task<Result<bool>> HandleAsync(
        UpdateMaintenancePlanCommand request,
        CancellationToken cancellationToken = default)
    {
        var plan = await _readRepository.GetByIdAsync(request.Id, cancellationToken);

        if (plan is null)
            throw new NotFoundException($"Maintenance plan '{request.Id}' was not found.");

        var changeContext = ChangeContext.Create("system", "Update maintenance plan");

        plan.Update(
            new MaintenancePlanName(request.Name),
            new MaintenancePlanDescription(request.Description),
            request.TargetType,
            request.TargetId,
            request.IntervalValue,
            request.IntervalUnit,
            request.NextDueAtUtc,
            changeContext);

        plan.SetActiveState(request.IsActive, changeContext);

        await _writeRepository.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}
