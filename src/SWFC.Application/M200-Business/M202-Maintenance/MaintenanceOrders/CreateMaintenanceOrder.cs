using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M200_Business.M202_Maintenance.MaintenanceOrders;

namespace SWFC.Application.M200_Business.M202_Maintenance.MaintenanceOrders;

public sealed record CreateMaintenanceOrderMaterialCommand(
    Guid ItemId,
    int Quantity);

public sealed record CreateMaintenanceOrderCommand(
    string Number,
    string Title,
    string Description,
    MaintenanceOrderType Type,
    MaintenanceTargetType TargetType,
    Guid TargetId,
    Guid? MaintenancePlanId,
    DateTime? PlannedStartUtc,
    DateTime? PlannedEndUtc,
    DateTime? DueAtUtc,
    IReadOnlyList<CreateMaintenanceOrderMaterialCommand> Materials);

public sealed class CreateMaintenanceOrderValidator : ICommandValidator<CreateMaintenanceOrderCommand>
{
    public Task<ValidationResult> ValidateAsync(
        CreateMaintenanceOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (string.IsNullOrWhiteSpace(command.Number))
            result.Add("M202.Order.Number.Required", "Number is required.");

        if (string.IsNullOrWhiteSpace(command.Title))
            result.Add("M202.Order.Title.Required", "Title is required.");

        if (string.IsNullOrWhiteSpace(command.Description))
            result.Add("M202.Order.Description.Required", "Description is required.");

        if (command.TargetId == Guid.Empty)
            result.Add("M202.Order.Target.Required", "Target id is required.");

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

public sealed class CreateMaintenanceOrderPolicy : IAuthorizationPolicy<CreateMaintenanceOrderCommand>
{
    public AuthorizationRequirement GetRequirement(CreateMaintenanceOrderCommand request)
    {
        return new AuthorizationRequirement(
            requiredPermissions: new[] { "m202.maintenance-orders.create" });
    }
}

public sealed class CreateMaintenanceOrderHandler : IUseCaseHandler<CreateMaintenanceOrderCommand, Guid>
{
    private readonly IMaintenanceOrderReadRepository _readRepository;
    private readonly IMaintenanceOrderWriteRepository _writeRepository;

    public CreateMaintenanceOrderHandler(
        IMaintenanceOrderReadRepository readRepository,
        IMaintenanceOrderWriteRepository writeRepository)
    {
        _readRepository = readRepository;
        _writeRepository = writeRepository;
    }

    public async Task<Result<Guid>> HandleAsync(
        CreateMaintenanceOrderCommand request,
        CancellationToken cancellationToken = default)
    {
        var numberExists = await _readRepository.NumberExistsAsync(
            request.Number,
            excludeId: null,
            cancellationToken);

        if (numberExists)
            throw new InvalidOperationException("Maintenance order number already exists.");

        var changeContext = ChangeContext.Create("system", "Create maintenance order");

        var order = MaintenanceOrder.Create(
            new MaintenanceOrderNumber(request.Number),
            new MaintenanceOrderTitle(request.Title),
            new MaintenanceOrderDescription(request.Description),
            request.Type,
            request.TargetType,
            request.TargetId,
            changeContext);

        foreach (var material in request.Materials)
        {
            order.AddMaterial(material.ItemId, material.Quantity, changeContext);
        }

        await _writeRepository.AddAsync(order, cancellationToken);
        await _writeRepository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(order.Id);
    }
}
