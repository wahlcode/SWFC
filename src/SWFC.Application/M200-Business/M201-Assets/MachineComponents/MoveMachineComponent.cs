using System.Text.Json;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.M100_System.M101_Foundation.Errors;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;

namespace SWFC.Application.M200_Business.M201_Assets.MachineComponents;

public sealed record MoveMachineComponentCommand(
    Guid Id,
    Guid TargetMachineId,
    Guid? TargetMachineComponentAreaId,
    Guid? ParentMachineComponentId,
    string Reason);

public sealed class MoveMachineComponentValidator : ICommandValidator<MoveMachineComponentCommand>
{
    public Task<ValidationResult> ValidateAsync(
        MoveMachineComponentCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.Id == Guid.Empty)
        {
            result.Add("Id", "Component id is invalid.");
        }

        if (command.TargetMachineId == Guid.Empty)
        {
            result.Add("TargetMachineId", "Target machine id is invalid.");
        }

        if (command.ParentMachineComponentId.HasValue && command.ParentMachineComponentId.Value == Guid.Empty)
        {
            result.Add("ParentMachineComponentId", "Parent component id is invalid.");
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            result.Add("Reason", "Reason is required.");
        }

        return Task.FromResult(result);
    }
}

public sealed class MoveMachineComponentPolicy : IAuthorizationPolicy<MoveMachineComponentCommand>
{
    public AuthorizationRequirement GetRequirement(MoveMachineComponentCommand request)
        => new(requiredPermissions: new[] { "machine.update" });
}

public sealed class MoveMachineComponentHandler : IUseCaseHandler<MoveMachineComponentCommand, bool>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IMachineComponentWriteRepository _writeRepository;
    private readonly IAuditService _auditService;

    public MoveMachineComponentHandler(
        ICurrentUserService currentUserService,
        IMachineComponentWriteRepository writeRepository,
        IAuditService auditService)
    {
        _currentUserService = currentUserService;
        _writeRepository = writeRepository;
        _auditService = auditService;
    }

    public async Task<Result<bool>> HandleAsync(
        MoveMachineComponentCommand command,
        CancellationToken cancellationToken = default)
    {
        var component = await _writeRepository.GetByIdAsync(command.Id, cancellationToken);

        if (component is null)
        {
            return Result<bool>.Failure(new Error(
                GeneralErrorCodes.NotFound,
                $"Machine component '{command.Id}' was not found.",
                ErrorCategory.NotFound));
        }

        SWFC.Domain.M200_Business.M201_Assets.MachineComponents.MachineComponent? parentComponent = null;

        if (command.ParentMachineComponentId.HasValue)
        {
            if (command.ParentMachineComponentId.Value == command.Id)
            {
                return Result<bool>.Failure(new Error(
                    ValidationErrorCodes.Invalid,
                    "Component cannot be its own parent.",
                    ErrorCategory.Validation));
            }

            parentComponent = await _writeRepository.GetByIdAsync(command.ParentMachineComponentId.Value, cancellationToken);

            if (parentComponent is null)
            {
                return Result<bool>.Failure(new Error(
                    GeneralErrorCodes.NotFound,
                    $"Parent component '{command.ParentMachineComponentId.Value}' was not found.",
                    ErrorCategory.NotFound));
            }
        }

        var descendantIds = await _writeRepository.GetDescendantIdsAsync(command.Id, cancellationToken);
        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);

        var oldValues = JsonSerializer.Serialize(new
        {
            component.Id,
            component.MachineId,
            component.MachineComponentAreaId,
            component.ParentMachineComponentId
        });

        component.Move(
            command.TargetMachineId,
            command.TargetMachineComponentAreaId,
            parentComponent,
            descendantIds,
            changeContext);

        var newValues = JsonSerializer.Serialize(new
        {
            component.Id,
            component.MachineId,
            component.MachineComponentAreaId,
            component.ParentMachineComponentId,
            command.Reason
        });

        await _auditService.WriteAsync(
            securityContext.UserId,
            securityContext.Username,
            "MoveMachineComponent",
            "MachineComponent",
            component.Id.ToString(),
            changeContext.ChangedAtUtc,
            oldValues,
            newValues,
            cancellationToken);

        await _writeRepository.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
