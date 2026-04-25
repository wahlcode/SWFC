using SWFC.Domain.M200_Business.M201_Assets.Errors;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.M100_System.M101_Foundation.Errors;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;

namespace SWFC.Application.M200_Business.M201_Assets.Machines;

public sealed record DeactivateMachineCommand(
    Guid Id,
    string Reason);

public sealed class DeactivateMachineValidator : ICommandValidator<DeactivateMachineCommand>
{
    public Task<ValidationResult> ValidateAsync(
        DeactivateMachineCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.Id == Guid.Empty)
        {
            result.Add(ValidationErrorCodes.Invalid, "Machine id is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            result.Add(MachineErrorCodes.ReasonRequired, "Reason is required.");
        }

        return Task.FromResult(result);
    }
}

public sealed class DeactivateMachinePolicy : IAuthorizationPolicy<DeactivateMachineCommand>
{
    public AuthorizationRequirement GetRequirement(DeactivateMachineCommand request)
    {
        return new AuthorizationRequirement(requiredPermissions: new[] { "machine.delete" });
    }
}

public sealed class DeactivateMachineHandler : IUseCaseHandler<DeactivateMachineCommand, bool>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IMachineWriteRepository _machineWriteRepository;
    private readonly IAuditService _auditService;

    public DeactivateMachineHandler(
        ICurrentUserService currentUserService,
        IMachineWriteRepository machineWriteRepository,
        IAuditService auditService)
    {
        _currentUserService = currentUserService;
        _machineWriteRepository = machineWriteRepository;
        _auditService = auditService;
    }

    public async Task<Result<bool>> HandleAsync(
        DeactivateMachineCommand command,
        CancellationToken cancellationToken = default)
    {
        var machine = await _machineWriteRepository.GetByIdWithChildrenAsync(command.Id, cancellationToken);

        if (machine is null)
        {
            return Result<bool>.Failure(new Error(
                GeneralErrorCodes.NotFound,
                $"Machine '{command.Id}' was not found.",
                ErrorCategory.NotFound));
        }

        if (machine.HasChildren())
        {
            return Result<bool>.Failure(new Error(
                ValidationErrorCodes.Invalid,
                "Machine cannot be deactivated because it has child machines.",
                ErrorCategory.Validation));
        }

        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);

        _machineWriteRepository.Deactivate(machine, changeContext);

        await _auditService.WriteAsync(
            userId: securityContext.UserId,
            username: securityContext.Username,
            action: "DeactivateMachine",
            entity: "Machine",
            entityId: machine.Id.ToString(),
            timestampUtc: changeContext.ChangedAtUtc,
            oldValues: null,
            newValues: """{"isActive":false}""",
            cancellationToken: cancellationToken);

        await _machineWriteRepository.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}


