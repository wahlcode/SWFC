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

public sealed record CreateMachineComponentAreaCommand(
    string Name,
    string Reason);

public sealed class CreateMachineComponentAreaValidator : ICommandValidator<CreateMachineComponentAreaCommand>
{
    public Task<ValidationResult> ValidateAsync(
        CreateMachineComponentAreaCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

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

public sealed class CreateMachineComponentAreaPolicy : IAuthorizationPolicy<CreateMachineComponentAreaCommand>
{
    public AuthorizationRequirement GetRequirement(CreateMachineComponentAreaCommand request)
        => new(requiredPermissions: new[] { "machine.update" });
}

public sealed class CreateMachineComponentAreaHandler : IUseCaseHandler<CreateMachineComponentAreaCommand, Guid>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IMachineComponentAreaWriteRepository _writeRepository;
    private readonly IAuditService _auditService;

    public CreateMachineComponentAreaHandler(
        ICurrentUserService currentUserService,
        IMachineComponentAreaWriteRepository writeRepository,
        IAuditService auditService)
    {
        _currentUserService = currentUserService;
        _writeRepository = writeRepository;
        _auditService = auditService;
    }

    public async Task<Result<Guid>> HandleAsync(
        CreateMachineComponentAreaCommand command,
        CancellationToken cancellationToken = default)
    {
        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);

        var area = MachineComponentArea.Create(
            MachineComponentAreaName.Create(command.Name),
            changeContext);

        await _writeRepository.AddAsync(area, cancellationToken);

        await _auditService.WriteAsync(
            securityContext.UserId,
            securityContext.Username,
            "CreateMachineComponentArea",
            "MachineComponentArea",
            area.Id.ToString(),
            changeContext.ChangedAtUtc,
            null,
            JsonSerializer.Serialize(new
            {
                area.Id,
                Name = area.Name.Value,
                area.IsActive,
                command.Reason
            }),
            cancellationToken);

        await _writeRepository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(area.Id);
    }
}
