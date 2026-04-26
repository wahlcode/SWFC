using System.Text.Json;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.M100_System.M101_Foundation.Errors;
using SWFC.Domain.M100_System.M101_Foundation.Exceptions;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M200_Business.M201_Assets.Machines;

namespace SWFC.Application.M200_Business.M201_Assets.Machines;

public sealed record UpdateMachineCommand(
    Guid Id,
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

public sealed class UpdateMachineValidator : ICommandValidator<UpdateMachineCommand>
{
    public Task<ValidationResult> ValidateAsync(
        UpdateMachineCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.Id == Guid.Empty)
        {
            result.Add("Id", "Machine id is invalid.");
        }

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

        if (command.ParentMachineId.HasValue && command.ParentMachineId.Value == command.Id)
        {
            result.Add("ParentMachineId", "Machine cannot be its own parent.");
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

public sealed class UpdateMachinePolicy : IAuthorizationPolicy<UpdateMachineCommand>
{
    public AuthorizationRequirement GetRequirement(UpdateMachineCommand request)
    {
        return new AuthorizationRequirement(requiredPermissions: new[] { "machine.update" });
    }
}

public sealed class UpdateMachineHandler : IUseCaseHandler<UpdateMachineCommand, bool>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IMachineWriteRepository _machineWriteRepository;
    private readonly IAuditService _auditService;

    public UpdateMachineHandler(
        ICurrentUserService currentUserService,
        IMachineWriteRepository machineWriteRepository,
        IAuditService auditService)
    {
        _currentUserService = currentUserService;
        _machineWriteRepository = machineWriteRepository;
        _auditService = auditService;
    }

    public async Task<Result<bool>> HandleAsync(
        UpdateMachineCommand command,
        CancellationToken cancellationToken = default)
    {
        var machine = await _machineWriteRepository.GetByIdAsync(command.Id, cancellationToken);

        if (machine is null)
        {
            return Result<bool>.Failure(new Error(
                GeneralErrorCodes.NotFound,
                $"Machine '{command.Id}' was not found.",
                ErrorCategory.NotFound));
        }

        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);

        var oldValues = JsonSerializer.Serialize(new
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
            machine.EnergyObjectId
        });

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

        machine.UpdateDetails(
            machineName,
            inventoryNumber,
            location,
            status,
            assetType,
            manufacturer,
            model,
            serialNumber,
            description,
            command.EnergyObjectId,
            changeContext);

        if (command.ParentMachineId.HasValue)
        {
            if (command.ParentMachineId == machine.Id)
            {
                throw new DomainException("Machine cannot be its own parent.");
            }

            var parent = await _machineWriteRepository.GetByIdAsync(command.ParentMachineId.Value, cancellationToken);

            if (parent is null)
            {
                return Result<bool>.Failure(new Error(
                    GeneralErrorCodes.NotFound,
                    $"Parent machine '{command.ParentMachineId.Value}' was not found.",
                    ErrorCategory.NotFound));
            }

            var descendantIds = await _machineWriteRepository.GetDescendantIdsAsync(machine.Id, cancellationToken);

            machine.AssignParent(parent, descendantIds, changeContext);
        }
        else
        {
            machine.AssignParent(null, Array.Empty<Guid>(), changeContext);
        }

        machine.AssignOrganizationUnit(command.OrganizationUnitId, changeContext);

        var newValues = JsonSerializer.Serialize(new
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
        });

        await _auditService.WriteAsync(
            userId: securityContext.UserId,
            username: securityContext.Username,
            action: "UpdateMachine",
            entity: "Machine",
            entityId: machine.Id.ToString(),
            timestampUtc: changeContext.ChangedAtUtc,
            oldValues: oldValues,
            newValues: newValues,
            cancellationToken: cancellationToken);

        await _machineWriteRepository.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
