using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Exceptions;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M200_Business.M205_Energy.EnergyMeters;

namespace SWFC.Application.M200_Business.M205_Energy.EnergyMeters;

public sealed record UpdateEnergyMeterCommand(
    Guid Id,
    string Name,
    EnergyMediumType MediumType,
    string Unit,
    bool IsManualEntryEnabled,
    bool IsExternalImportEnabled,
    string? ExternalSystem,
    string? RfidTag,
    bool SupportsOfflineCapture,
    Guid? MachineId,
    bool IsActive,
    string Reason);

public sealed class UpdateEnergyMeterValidator : ICommandValidator<UpdateEnergyMeterCommand>
{
    public Task<ValidationResult> ValidateAsync(
        UpdateEnergyMeterCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.Id == Guid.Empty)
            result.Add("M205.Meter.Id.Required", "Id is required.");

        if (string.IsNullOrWhiteSpace(command.Name))
            result.Add("M205.Meter.Name.Required", "Name is required.");

        if (string.IsNullOrWhiteSpace(command.Unit))
            result.Add("M205.Meter.Unit.Required", "Unit is required.");

        if (!command.IsManualEntryEnabled && !command.IsExternalImportEnabled)
            result.Add("M205.Meter.Mode.Required", "At least one meter mode is required.");

        if (command.IsExternalImportEnabled && string.IsNullOrWhiteSpace(command.ExternalSystem))
            result.Add("M205.Meter.ExternalSystem.Required", "External system is required when external import is enabled.");

        if (command.SupportsOfflineCapture && !command.IsManualEntryEnabled)
            result.Add("M205.Meter.Offline.RequiresManualEntry", "Offline capture requires manual entry support.");

        if (string.IsNullOrWhiteSpace(command.Reason))
            result.Add("M205.Meter.Reason.Required", "Reason is required.");

        return Task.FromResult(result);
    }
}

public sealed class UpdateEnergyMeterPolicy : IAuthorizationPolicy<UpdateEnergyMeterCommand>
{
    public AuthorizationRequirement GetRequirement(UpdateEnergyMeterCommand request)
    {
        return new AuthorizationRequirement(
            requiredPermissions: new[] { "m205.energy-meters.update" });
    }
}

public sealed class UpdateEnergyMeterHandler : IUseCaseHandler<UpdateEnergyMeterCommand, Guid>
{
    private readonly IEnergyMeterReadRepository _readRepository;
    private readonly IEnergyMeterWriteRepository _writeRepository;

    public UpdateEnergyMeterHandler(
        IEnergyMeterReadRepository readRepository,
        IEnergyMeterWriteRepository writeRepository)
    {
        _readRepository = readRepository;
        _writeRepository = writeRepository;
    }

    public async Task<Result<Guid>> HandleAsync(
        UpdateEnergyMeterCommand request,
        CancellationToken cancellationToken = default)
    {
        var meter = await _readRepository.GetByIdAsync(request.Id, cancellationToken);

        if (meter is null)
            throw new NotFoundException($"Energy meter '{request.Id}' was not found.");

        var changeContext = ChangeContext.Create("system", request.Reason);

        meter.Update(
            new EnergyMeterName(request.Name),
            request.MediumType,
            new EnergyMeterUnit(request.Unit),
            request.IsManualEntryEnabled,
            request.IsExternalImportEnabled,
            EnergyExternalSystem.CreateOptional(request.ExternalSystem),
            EnergyMeterRfidTag.CreateOptional(request.RfidTag),
            request.SupportsOfflineCapture,
            request.MachineId,
            changeContext);

        meter.SetActiveState(request.IsActive, changeContext);

        await _writeRepository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(meter.Id);
    }
}
