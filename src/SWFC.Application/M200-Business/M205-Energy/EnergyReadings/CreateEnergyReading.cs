using SWFC.Application.M200_Business.M205_Energy.EnergyMeters;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Exceptions;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M200_Business.M205_Energy.EnergyReadings;

namespace SWFC.Application.M200_Business.M205_Energy.EnergyReadings;

public sealed record CreateEnergyReadingCommand(
    Guid MeterId,
    DateTime Date,
    decimal Value,
    EnergyReadingSource Source,
    string Reason);

public sealed class CreateEnergyReadingValidator : ICommandValidator<CreateEnergyReadingCommand>
{
    public Task<ValidationResult> ValidateAsync(
        CreateEnergyReadingCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.MeterId == Guid.Empty)
            result.Add("M205.Reading.Meter.Required", "Meter id is required.");

        if (command.Value < 0)
            result.Add("M205.Reading.Value.Invalid", "Value must not be negative.");

        if (string.IsNullOrWhiteSpace(command.Reason))
            result.Add("M205.Reading.Reason.Required", "Reason is required.");

        return Task.FromResult(result);
    }
}

public sealed class CreateEnergyReadingPolicy : IAuthorizationPolicy<CreateEnergyReadingCommand>
{
    public AuthorizationRequirement GetRequirement(CreateEnergyReadingCommand request)
    {
        return new AuthorizationRequirement(
            requiredPermissions: new[] { "m205.energy-readings.create" });
    }
}

public sealed class CreateEnergyReadingHandler : IUseCaseHandler<CreateEnergyReadingCommand, Guid>
{
    private readonly IEnergyMeterReadRepository _meterReadRepository;
    private readonly IEnergyReadingWriteRepository _writeRepository;

    public CreateEnergyReadingHandler(
        IEnergyMeterReadRepository meterReadRepository,
        IEnergyReadingWriteRepository writeRepository)
    {
        _meterReadRepository = meterReadRepository;
        _writeRepository = writeRepository;
    }

    public async Task<Result<Guid>> HandleAsync(
        CreateEnergyReadingCommand request,
        CancellationToken cancellationToken = default)
    {
        var meter = await _meterReadRepository.GetByIdAsync(request.MeterId, cancellationToken);

        if (meter is null)
            throw new NotFoundException($"Energy meter '{request.MeterId}' was not found.");

        var changeContext = ChangeContext.Create("system", request.Reason);

        var reading = EnergyReading.Create(
            request.MeterId,
            new EnergyReadingDate(request.Date),
            new EnergyReadingValue(request.Value),
            request.Source,
            changeContext);

        await _writeRepository.AddAsync(reading, cancellationToken);
        await _writeRepository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(reading.Id);
    }
}
