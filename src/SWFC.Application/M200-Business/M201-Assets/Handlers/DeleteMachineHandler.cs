using System.Text.Json;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Application.M200_Business.M201_Assets.Commands;
using SWFC.Application.M200_Business.M201_Assets.Interfaces;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Domain.Common.Errors;
using SWFC.Domain.Common.Results;
using SWFC.Domain.Common.ValueObjects;

namespace SWFC.Application.M200_Business.M201_Assets.Handlers;

public sealed class DeleteMachineHandler : IUseCaseHandler<DeleteMachineCommand, bool>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IMachineWriteRepository _machineWriteRepository;
    private readonly IAuditService _auditService;

    public DeleteMachineHandler(
        ICurrentUserService currentUserService,
        IMachineWriteRepository machineWriteRepository,
        IAuditService auditService)
    {
        _currentUserService = currentUserService;
        _machineWriteRepository = machineWriteRepository;
        _auditService = auditService;
    }

    public async Task<Result<bool>> HandleAsync(
        DeleteMachineCommand command,
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
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);

        var oldValues = JsonSerializer.Serialize(new
        {
            machine.Id,
            Name = machine.Name.Value,
            machine.AuditInfo.CreatedAtUtc,
            machine.AuditInfo.CreatedBy,
            machine.AuditInfo.LastModifiedAtUtc,
            machine.AuditInfo.LastModifiedBy,
            command.Reason
        });

        _machineWriteRepository.Remove(machine);

        await _auditService.WriteAsync(
            userId: securityContext.UserId,
            username: securityContext.Username,
            action: "DeleteMachine",
            entity: "Machine",
            entityId: machine.Id.ToString(),
            timestampUtc: changeContext.ChangedAtUtc,
            oldValues: oldValues,
            newValues: null,
            cancellationToken: cancellationToken);

        await _machineWriteRepository.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}