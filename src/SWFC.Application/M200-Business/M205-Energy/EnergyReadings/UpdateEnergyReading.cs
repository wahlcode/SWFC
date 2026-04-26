using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Exceptions;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M200_Business.M205_Energy.EnergyReadings;

namespace SWFC.Application.M200_Business.M205_Energy.EnergyReadings;

public sealed record UpdateEnergyReadingCommand(
    Guid Id,
    DateTime Date,
    decimal Value,
    EnergyReadingSource Source,
    string? CapturedByUserId,
    string? CaptureContext,
    string? RfidTag,
    string? RfidExceptionReason,
    Guid? OfflineCaptureId,
    DateTime? CapturedOfflineAtUtc,
    DateTime? SyncedAtUtc,
    EnergyReadingPlausibilityStatus PlausibilityStatus,
    string? PlausibilityNote,
    string Reason);

public sealed class UpdateEnergyReadingValidator : ICommandValidator<UpdateEnergyReadingCommand>
{
    public Task<ValidationResult> ValidateAsync(
        UpdateEnergyReadingCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.Id == Guid.Empty)
            result.Add("M205.Reading.Id.Required", "Id is required.");

        if (command.Value < 0)
            result.Add("M205.Reading.Value.Invalid", "Value must not be negative.");

        if (command.Source == EnergyReadingSource.Manual
            && string.IsNullOrWhiteSpace(command.CapturedByUserId))
            result.Add("M205.Reading.User.Required", "Manual readings require a capturing user.");

        if (command.Source == EnergyReadingSource.Manual
            && string.IsNullOrWhiteSpace(command.RfidTag)
            && string.IsNullOrWhiteSpace(command.RfidExceptionReason))
            result.Add("M205.Reading.Rfid.Required", "Manual readings require RFID or an exception reason.");

        if (command.CapturedOfflineAtUtc.HasValue && !command.OfflineCaptureId.HasValue)
            result.Add("M205.Reading.Offline.Id.Required", "Offline captures require a stable offline capture id.");

        if (command.PlausibilityStatus == EnergyReadingPlausibilityStatus.Flagged
            && string.IsNullOrWhiteSpace(command.PlausibilityNote))
            result.Add("M205.Reading.Plausibility.Note.Required", "Flagged readings require a plausibility note.");

        if (string.IsNullOrWhiteSpace(command.Reason))
            result.Add("M205.Reading.Reason.Required", "Reason is required.");

        return Task.FromResult(result);
    }
}

public sealed class UpdateEnergyReadingPolicy : IAuthorizationPolicy<UpdateEnergyReadingCommand>
{
    public AuthorizationRequirement GetRequirement(UpdateEnergyReadingCommand request)
    {
        return new AuthorizationRequirement(
            requiredPermissions: new[] { "m205.energy-readings.update" });
    }
}

public sealed class UpdateEnergyReadingHandler : IUseCaseHandler<UpdateEnergyReadingCommand, Guid>
{
    private readonly IEnergyReadingReadRepository _readRepository;
    private readonly IEnergyReadingWriteRepository _writeRepository;

    public UpdateEnergyReadingHandler(
        IEnergyReadingReadRepository readRepository,
        IEnergyReadingWriteRepository writeRepository)
    {
        _readRepository = readRepository;
        _writeRepository = writeRepository;
    }

    public async Task<Result<Guid>> HandleAsync(
        UpdateEnergyReadingCommand request,
        CancellationToken cancellationToken = default)
    {
        var reading = await _readRepository.GetByIdAsync(request.Id, cancellationToken);

        if (reading is null)
            throw new NotFoundException($"Energy reading '{request.Id}' was not found.");

        var changeContext = ChangeContext.Create("system", request.Reason);

        reading.Update(
            new EnergyReadingDate(request.Date),
            new EnergyReadingValue(request.Value),
            request.Source,
            request.CapturedByUserId,
            EnergyReadingCaptureContext.CreateOptional(request.CaptureContext),
            EnergyReadingRfidTag.CreateOptional(request.RfidTag),
            EnergyReadingRfidExceptionReason.CreateOptional(request.RfidExceptionReason),
            request.OfflineCaptureId,
            request.CapturedOfflineAtUtc,
            request.SyncedAtUtc,
            request.PlausibilityStatus,
            EnergyReadingPlausibilityNote.CreateOptional(request.PlausibilityNote),
            changeContext);

        await _writeRepository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(reading.Id);
    }
}
