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
            changeContext);

        await _writeRepository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(reading.Id);
    }
}
