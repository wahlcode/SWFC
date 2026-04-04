using System.Text.Json;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Application.M200_Business.M201_Assets.Commands;
using SWFC.Application.M200_Business.M201_Assets.Interfaces;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Domain.Common.Errors;
using SWFC.Domain.Common.Results;
using SWFC.Domain.Common.ValueObjects;
using SWFC.Domain.M200_Business.M201_Assets.ValueObjects;

namespace SWFC.Application.M200_Business.M201_Assets.Handlers;

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
                ErrorCodes.General.NotFound,
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
            Manufacturer = machine.Manufacturer.Value,
            Model = machine.Model.Value,
            SerialNumber = machine.SerialNumber.Value,
            Description = machine.Description.Value,
            machine.AuditInfo.CreatedAtUtc,
            machine.AuditInfo.CreatedBy,
            machine.AuditInfo.LastModifiedAtUtc,
            machine.AuditInfo.LastModifiedBy
        });

        var machineName = MachineName.Create(command.Name);
        var inventoryNumber = MachineInventoryNumber.Create(command.InventoryNumber);
        var location = MachineLocation.Create(command.Location);
        var status = MachineStatus.Create(command.Status);
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
            manufacturer,
            model,
            serialNumber,
            description,
            changeContext);

        var newValues = JsonSerializer.Serialize(new
        {
            machine.Id,
            Name = machine.Name.Value,
            InventoryNumber = machine.InventoryNumber.Value,
            Location = machine.Location.Value,
            Status = machine.Status.Value,
            Manufacturer = machine.Manufacturer.Value,
            Model = machine.Model.Value,
            SerialNumber = machine.SerialNumber.Value,
            Description = machine.Description.Value,
            machine.AuditInfo.CreatedAtUtc,
            machine.AuditInfo.CreatedBy,
            machine.AuditInfo.LastModifiedAtUtc,
            machine.AuditInfo.LastModifiedBy,
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