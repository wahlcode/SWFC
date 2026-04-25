using System.Text.Json;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.M100_System.M101_Foundation.Errors;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M200_Business.M201_Assets.MachineComponentAreas;

namespace SWFC.Application.M200_Business.M201_Assets.MachineComponentAreas;

public sealed record UpdateMachineComponentAreaCommand(
    Guid Id,
    string Name,
    string Reason);

public sealed class UpdateMachineComponentAreaValidator : ICommandValidator<UpdateMachineComponentAreaCommand>
{
    public Task<ValidationResult> ValidateAsync(
        UpdateMachineComponentAreaCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.Id == Guid.Empty)
        {
            result.Add("Id", "Area id is invalid.");
        }

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            result.Add("Name", "Area name is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            result.Add("Reason", "Reason is required.");
        }

        return Task.FromResult(result);
    }
}

public sealed class UpdateMachineComponentAreaPolicy : IAuthorizationPolicy<UpdateMachineComponentAreaCommand>
{
    public AuthorizationRequirement GetRequirement(UpdateMachineComponentAreaCommand request)
        => new(requiredPermissions: new[] { "machine.update" });
}

public sealed class UpdateMachineComponentAreaHandler : IUseCaseHandler<UpdateMachineComponentAreaCommand, bool>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IMachineComponentAreaWriteRepository _writeRepository;
    private readonly IAuditService _auditService;

    public UpdateMachineComponentAreaHandler(
        ICurrentUserService currentUserService,
        IMachineComponentAreaWriteRepository writeRepository,
        IAuditService auditService)
    {
        _currentUserService = currentUserService;
        _writeRepository = writeRepository;
        _auditService = auditService;
    }

    public async Task<Result<bool>> HandleAsync(
        UpdateMachineComponentAreaCommand command,
        CancellationToken cancellationToken = default)
    {
        var area = await _writeRepository.GetByIdAsync(command.Id, cancellationToken);

        if (area is null)
        {
            return Result<bool>.Failure(new Error(
                GeneralErrorCodes.NotFound,
                $"Machine component area '{command.Id}' was not found.",
                ErrorCategory.NotFound));
        }

        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);

        var oldValues = JsonSerializer.Serialize(new
        {
            area.Id,
            Name = area.Name.Value,
            area.IsActive
        });

        area.Update(
            MachineComponentAreaName.Create(command.Name),
            changeContext);

        var newValues = JsonSerializer.Serialize(new
        {
            area.Id,
            Name = area.Name.Value,
            area.IsActive,
            command.Reason
        });

        await _auditService.WriteAsync(
            securityContext.UserId,
            securityContext.Username,
            "UpdateMachineComponentArea",
            "MachineComponentArea",
            area.Id.ToString(),
            changeContext.ChangedAtUtc,
            oldValues,
            newValues,
            cancellationToken);

        await _writeRepository.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
