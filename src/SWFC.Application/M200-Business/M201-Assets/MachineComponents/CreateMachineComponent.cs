using System.Text.Json;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.M100_System.M101_Foundation.Errors;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M200_Business.M201_Assets.MachineComponents;

namespace SWFC.Application.M200_Business.M201_Assets.MachineComponents;

public sealed record CreateMachineComponentCommand(
    Guid MachineId,
    Guid? MachineComponentAreaId,
    Guid? ParentMachineComponentId,
    string Name,
    string? Description,
    string Reason);

public sealed class CreateMachineComponentValidator : ICommandValidator<CreateMachineComponentCommand>
{
    public Task<ValidationResult> ValidateAsync(
        CreateMachineComponentCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.MachineId == Guid.Empty)
        {
            result.Add("MachineId", "Machine id is required.");
        }

        if (command.ParentMachineComponentId.HasValue && command.ParentMachineComponentId.Value == Guid.Empty)
        {
            result.Add("ParentMachineComponentId", "Parent component id is invalid.");
        }

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            result.Add("Name", "Component name is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            result.Add("Reason", "Reason is required.");
        }

        return Task.FromResult(result);
    }
}

public sealed class CreateMachineComponentPolicy : IAuthorizationPolicy<CreateMachineComponentCommand>
{
    public AuthorizationRequirement GetRequirement(CreateMachineComponentCommand request)
        => new(requiredPermissions: new[] { "machine.update" });
}

public sealed class CreateMachineComponentHandler : IUseCaseHandler<CreateMachineComponentCommand, Guid>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IMachineComponentWriteRepository _writeRepository;
    private readonly IAuditService _auditService;

    public CreateMachineComponentHandler(
        ICurrentUserService currentUserService,
        IMachineComponentWriteRepository writeRepository,
        IAuditService auditService)
    {
        _currentUserService = currentUserService;
        _writeRepository = writeRepository;
        _auditService = auditService;
    }

    public async Task<Result<Guid>> HandleAsync(
        CreateMachineComponentCommand command,
        CancellationToken cancellationToken = default)
    {
        MachineComponent? parentComponent = null;

        if (command.ParentMachineComponentId.HasValue)
        {
            parentComponent = await _writeRepository.GetByIdAsync(command.ParentMachineComponentId.Value, cancellationToken);

            if (parentComponent is null)
            {
                return Result<Guid>.Failure(new Error(
                    GeneralErrorCodes.NotFound,
                    $"Parent component '{command.ParentMachineComponentId.Value}' was not found.",
                    ErrorCategory.NotFound));
            }
        }

        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);

        var component = MachineComponent.Create(
            command.MachineId,
            command.MachineComponentAreaId,
            parentComponent,
            MachineComponentName.Create(command.Name),
            MachineComponentDescription.Create(command.Description),
            changeContext);

        await _writeRepository.AddAsync(component, cancellationToken);

        await _auditService.WriteAsync(
            securityContext.UserId,
            securityContext.Username,
            "CreateMachineComponent",
            "MachineComponent",
            component.Id.ToString(),
            changeContext.ChangedAtUtc,
            null,
            JsonSerializer.Serialize(new
            {
                component.Id,
                component.MachineId,
                component.MachineComponentAreaId,
                component.ParentMachineComponentId,
                Name = component.Name.Value,
                Description = component.Description.Value,
                component.IsActive,
                command.Reason
            }),
            cancellationToken);

        await _writeRepository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(component.Id);
    }
}
