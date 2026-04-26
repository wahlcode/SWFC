using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M200_Business.M205_Energy.EnergyMeters;

namespace SWFC.Application.M200_Business.M205_Energy.EnergyMeters;

public sealed record CreateEnergyMeterCommand(
    string Name,
    EnergyMediumType MediumType,
    string MediumName,
    string Unit,
    bool IsManualEntryEnabled,
    bool IsExternalImportEnabled,
    string? ExternalSystem,
    string? RfidTag,
    bool SupportsOfflineCapture,
    Guid? ParentMeterId,
    Guid? MachineId,
    string Reason);

public sealed class CreateEnergyMeterValidator : ICommandValidator<CreateEnergyMeterCommand>
{
    public Task<ValidationResult> ValidateAsync(
        CreateEnergyMeterCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (string.IsNullOrWhiteSpace(command.Name))
            result.Add("M205.Meter.Name.Required", "Name is required.");

        if (string.IsNullOrWhiteSpace(command.Unit))
            result.Add("M205.Meter.Unit.Required", "Unit is required.");

        if (string.IsNullOrWhiteSpace(command.MediumName))
            result.Add("M205.Meter.MediumName.Required", "Medium name is required.");

        if (!command.IsManualEntryEnabled && !command.IsExternalImportEnabled)
            result.Add("M205.Meter.Mode.Required", "At least one meter mode is required.");

        if (command.IsExternalImportEnabled && string.IsNullOrWhiteSpace(command.ExternalSystem))
            result.Add("M205.Meter.ExternalSystem.Required", "External system is required when external import is enabled.");

        if (command.SupportsOfflineCapture && !command.IsManualEntryEnabled)
            result.Add("M205.Meter.Offline.RequiresManualEntry", "Offline capture requires manual entry support.");

        if (command.ParentMeterId.HasValue && command.ParentMeterId == command.MachineId)
            result.Add("M205.Meter.Parent.Invalid", "Parent meter id must be distinct from linked machine id.");

        if (string.IsNullOrWhiteSpace(command.Reason))
            result.Add("M205.Meter.Reason.Required", "Reason is required.");

        return Task.FromResult(result);
    }
}

public sealed class CreateEnergyMeterPolicy : IAuthorizationPolicy<CreateEnergyMeterCommand>
{
    public AuthorizationRequirement GetRequirement(CreateEnergyMeterCommand request)
    {
        return new AuthorizationRequirement(
            requiredPermissions: new[] { "m205.energy-meters.create" });
    }
}

public sealed class CreateEnergyMeterHandler : IUseCaseHandler<CreateEnergyMeterCommand, Guid>
{
    private readonly IEnergyMeterWriteRepository _writeRepository;

    public CreateEnergyMeterHandler(IEnergyMeterWriteRepository writeRepository)
    {
        _writeRepository = writeRepository;
    }

    public async Task<Result<Guid>> HandleAsync(
        CreateEnergyMeterCommand request,
        CancellationToken cancellationToken = default)
    {
        var changeContext = ChangeContext.Create("system", request.Reason);

        var meter = EnergyMeter.Create(
            new EnergyMeterName(request.Name),
            request.MediumType,
            new EnergyMediumName(request.MediumName),
            new EnergyMeterUnit(request.Unit),
            request.IsManualEntryEnabled,
            request.IsExternalImportEnabled,
            EnergyExternalSystem.CreateOptional(request.ExternalSystem),
            EnergyMeterRfidTag.CreateOptional(request.RfidTag),
            request.SupportsOfflineCapture,
            request.ParentMeterId,
            request.MachineId,
            changeContext);

        await _writeRepository.AddAsync(meter, cancellationToken);
        await _writeRepository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(meter.Id);
    }
}
