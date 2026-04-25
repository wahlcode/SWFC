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

public sealed record SetMachineComponentActiveStateCommand(
    Guid Id,
    bool IsActive,
    string Reason);

public sealed class SetMachineComponentActiveStateValidator : ICommandValidator<SetMachineComponentActiveStateCommand>
{
    public Task<ValidationResult> ValidateAsync(
        SetMachineComponentActiveStateCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.Id == Guid.Empty)
        {
            result.Add("Id", "Component id is invalid.");
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            result.Add("Reason", "Reason is required.");
        }

        return Task.FromResult(result);
    }
}

public sealed class SetMachineComponentActiveStatePolicy : IAuthorizationPolicy<SetMachineComponentActiveStateCommand>
{
    public AuthorizationRequirement GetRequirement(SetMachineComponentActiveStateCommand request)
        => new(requiredPermissions: new[] { "machine.update" });
}

public sealed class SetMachineComponentActiveStateHandler : IUseCaseHandler<SetMachineComponentActiveStateCommand, bool>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IMachineComponentWriteRepository _writeRepository;
    private readonly IAuditService _auditService;

    public SetMachineComponentActiveStateHandler(
        ICurrentUserService currentUserService,
        IMachineComponentWriteRepository writeRepository,
        IAuditService auditService)
    {
        _currentUserService = currentUserService;
        _writeRepository = writeRepository;
        _auditService = auditService;
    }

    public async Task<Result<bool>> HandleAsync(
        SetMachineComponentActiveStateCommand command,
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

        var hasChildren = await _writeRepository.HasChildrenAsync(command.Id, cancellationToken);
        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);

        var oldValues = JsonSerializer.Serialize(new
        {
            component.Id,
            component.IsActive
        });

        component.SetActiveState(command.IsActive, hasChildren, changeContext);

        var newValues = JsonSerializer.Serialize(new
        {
            component.Id,
            component.IsActive,
            command.Reason
        });

        await _auditService.WriteAsync(
            securityContext.UserId,
            securityContext.Username,
            "SetMachineComponentActiveState",
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
