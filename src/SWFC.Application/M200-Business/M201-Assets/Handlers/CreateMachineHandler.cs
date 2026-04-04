using System.Text.Json;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Application.M200_Business.M201_Assets.Commands;
using SWFC.Application.M200_Business.M201_Assets.Interfaces;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Domain.Common.Results;
using SWFC.Domain.Common.ValueObjects;
using SWFC.Domain.M200_Business.M201_Assets.Entities;
using SWFC.Domain.M200_Business.M201_Assets.ValueObjects;

namespace SWFC.Application.M200_Business.M201_Assets.Handlers;

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
        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);

        var machineName = MachineName.Create(command.Name);
        var inventoryNumber = MachineInventoryNumber.Create(command.InventoryNumber);
        var location = MachineLocation.Create(command.Location);
        var status = MachineStatus.Create(command.Status);
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
            manufacturer,
            model,
            serialNumber,
            description,
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
                Manufacturer = machine.Manufacturer.Value,
                Model = machine.Model.Value,
                SerialNumber = machine.SerialNumber.Value,
                Description = machine.Description.Value,
                command.Reason
            }),
            cancellationToken: cancellationToken);

        await _machineWriteRepository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(machine.Id);
    }
}