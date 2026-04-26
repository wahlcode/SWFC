using System.Text.Json;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.M100_System.M101_Foundation.Errors;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M200_Business.M201_Assets.Machines;

namespace SWFC.Application.M200_Business.M201_Assets.Machines;

public sealed record CreateMachineCommand(
    string Name,
    string InventoryNumber,
    string? Location,
    string Status,
    string? AssetType,
    string? Manufacturer,
    string? Model,
    string? SerialNumber,
    string? Description,
    Guid? ParentMachineId,
    Guid? OrganizationUnitId,
    Guid? EnergyObjectId,
    string Reason);

public sealed class CreateMachineValidator : ICommandValidator<CreateMachineCommand>
{
    public Task<ValidationResult> ValidateAsync(
        CreateMachineCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            result.Add("Name", "Machine name is required.");
        }

        if (string.IsNullOrWhiteSpace(command.InventoryNumber))
        {
            result.Add("InventoryNumber", "Inventory number is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Status))
        {
            result.Add("Status", "Machine status is required.");
        }

        if (command.ParentMachineId.HasValue && command.ParentMachineId.Value == Guid.Empty)
        {
            result.Add("ParentMachineId", "Parent machine id is invalid.");
        }

        if (command.OrganizationUnitId.HasValue && command.OrganizationUnitId.Value == Guid.Empty)
        {
            result.Add("OrganizationUnitId", "Organization unit id is invalid.");
        }

        if (command.EnergyObjectId.HasValue && command.EnergyObjectId.Value == Guid.Empty)
        {
            result.Add("EnergyObjectId", "Energy object id is invalid.");
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            result.Add("Reason", "Reason is required.");
        }

        return Task.FromResult(result);
    }
}

public sealed class CreateMachinePolicy : IAuthorizationPolicy<CreateMachineCommand>
{
    public AuthorizationRequirement GetRequirement(CreateMachineCommand request)
    {
        return new AuthorizationRequirement(requiredPermissions: new[] { "machine.create" });
    }
}

public sealed class CreateMachineHandler : IUseCaseHandler<CreateMachineCommand, Guid>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IMachineWriteRepository _machineWriteRepository;
    private readonly IAuditService _auditService;

    public CreateMachineHandler(
        ICurrentUserService currentUserService,
        IMachineWriteRepository machineWriteRepository,
        IAuditService auditService)
    {
        _currentUserService = currentUserService;
        _machineWriteRepository = machineWriteRepository;
        _auditService = auditService;
    }

    public async Task<Result<Guid>> HandleAsync(
        CreateMachineCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.ParentMachineId.HasValue)
        {
            var parent = await _machineWriteRepository.GetByIdAsync(command.ParentMachineId.Value, cancellationToken);

            if (parent is null)
            {
                return Result<Guid>.Failure(new Error(
                    GeneralErrorCodes.NotFound,
                    $"Parent machine '{command.ParentMachineId.Value}' was not found.",
                    ErrorCategory.NotFound));
            }
        }

        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);

        var machineName = MachineName.Create(command.Name);
        var inventoryNumber = MachineInventoryNumber.Create(command.InventoryNumber);
        var location = MachineLocation.Create(command.Location);
        var status = MachineStatus.Create(command.Status);
        var assetType = MachineAssetType.Create(command.AssetType);
        var manufacturer = MachineManufacturer.Create(command.Manufacturer);
        var model = MachineModel.Create(command.Model);
        var serialNumber = MachineSerialNumber.Create(command.SerialNumber);
        var description = MachineDescription.Create(command.Description);

        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);

        var machine = Machine.Create(
            machineName,
            inventoryNumber,
            location,
            status,
            assetType,
            manufacturer,
            model,
            serialNumber,
            description,
            command.ParentMachineId,
            command.OrganizationUnitId,
            command.EnergyObjectId,
            changeContext);

        await _machineWriteRepository.AddAsync(machine, cancellationToken);

        await _auditService.WriteAsync(
            userId: securityContext.UserId,
            username: securityContext.Username,
            action: "CreateMachine",
            entity: "Machine",
            entityId: machine.Id.ToString(),
            timestampUtc: changeContext.ChangedAtUtc,
            oldValues: null,
            newValues: JsonSerializer.Serialize(new
            {
                machine.Id,
                Name = machine.Name.Value,
                InventoryNumber = machine.InventoryNumber.Value,
                Location = machine.Location.Value,
                Status = machine.Status.Value,
                AssetType = machine.AssetType.Value,
                Manufacturer = machine.Manufacturer.Value,
                Model = machine.Model.Value,
                SerialNumber = machine.SerialNumber.Value,
                Description = machine.Description.Value,
                machine.ParentMachineId,
                machine.OrganizationUnitId,
                machine.EnergyObjectId,
                command.Reason
            }),
            cancellationToken: cancellationToken);

        await _machineWriteRepository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(machine.Id);
    }
}

