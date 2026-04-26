using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Exceptions;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M200_Business.M202_Maintenance.MaintenanceOrders;

namespace SWFC.Application.M200_Business.M202_Maintenance.MaintenanceOrders;

public sealed record UpdateMaintenanceOrderMaterialCommand(
    Guid ItemId,
    int Quantity);

public sealed record UpdateMaintenanceOrderCommand(
    Guid Id,
    string Title,
    string Description,
    MaintenanceOrderType Type,
    MaintenanceOrderStatus Status,
    MaintenanceOrderPriority Priority,
    MaintenanceTargetType TargetType,
    Guid TargetId,
    Guid? MaintenancePlanId,
    DateTime? PlannedStartUtc,
    DateTime? PlannedEndUtc,
    DateTime? DueAtUtc,
    IReadOnlyList<UpdateMaintenanceOrderMaterialCommand> Materials);

public sealed class UpdateMaintenanceOrderValidator : ICommandValidator<UpdateMaintenanceOrderCommand>
{
    public Task<ValidationResult> ValidateAsync(
        UpdateMaintenanceOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.Id == Guid.Empty)
            result.Add("M202.Order.Id.Required", "Id is required.");

        if (string.IsNullOrWhiteSpace(command.Title))
            result.Add("M202.Order.Title.Required", "Title is required.");

        if (string.IsNullOrWhiteSpace(command.Description))
            result.Add("M202.Order.Description.Required", "Description is required.");

        if (command.TargetId == Guid.Empty)
            result.Add("M202.Order.Target.Required", "Target id is required.");

        if (command.MaintenancePlanId == Guid.Empty)
            result.Add("M202.Order.Plan.Invalid", "Maintenance plan id must not be empty.");

        if (command.PlannedStartUtc.HasValue &&
            command.PlannedEndUtc.HasValue &&
            command.PlannedEndUtc.Value < command.PlannedStartUtc.Value)
            result.Add("M202.Order.PlanningWindow.Invalid", "Planned end must not be before planned start.");

        foreach (var material in command.Materials)
        {
            if (material.ItemId == Guid.Empty)
                result.Add("M202.Order.Material.Item.Required", "Material item id is required.");

            if (material.Quantity <= 0)
                result.Add("M202.Order.Material.Quantity.Invalid", "Material quantity must be greater than zero.");
        }

        return Task.FromResult(result);
    }
}

public sealed class UpdateMaintenanceOrderPolicy : IAuthorizationPolicy<UpdateMaintenanceOrderCommand>
{
    public AuthorizationRequirement GetRequirement(UpdateMaintenanceOrderCommand request)
    {
        return new AuthorizationRequirement(
            requiredPermissions: new[] { "m202.maintenance-orders.update" });
    }
}

public sealed class UpdateMaintenanceOrderHandler : IUseCaseHandler<UpdateMaintenanceOrderCommand, bool>
{
    private readonly IMaintenanceOrderReadRepository _readRepository;
    private readonly IMaintenanceOrderWriteRepository _writeRepository;

    public UpdateMaintenanceOrderHandler(
        IMaintenanceOrderReadRepository readRepository,
        IMaintenanceOrderWriteRepository writeRepository)
    {
        _readRepository = readRepository;
        _writeRepository = writeRepository;
    }

    public async Task<Result<bool>> HandleAsync(
        UpdateMaintenanceOrderCommand request,
        CancellationToken cancellationToken = default)
    {
        var order = await _readRepository.GetByIdAsync(request.Id, cancellationToken);

        if (order is null)
            throw new NotFoundException($"Maintenance order '{request.Id}' was not found.");

        var changeContext = ChangeContext.Create("system", "Update maintenance order");

        order.Update(
            new MaintenanceOrderTitle(request.Title),
            new MaintenanceOrderDescription(request.Description),
            request.Type,
            request.Priority,
            request.Status,
            request.TargetType,
            request.TargetId,
            request.MaintenancePlanId,
            request.PlannedStartUtc,
            request.PlannedEndUtc,
            request.DueAtUtc,
            request.Materials.Select(x => (x.ItemId, x.Quantity)).ToArray(),
            changeContext);

        await _writeRepository.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
