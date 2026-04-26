using SWFC.Application.M200_Business.M205_Energy.EnergyMeters;
using SWFC.Application.M200_Business.M205_Energy.EnergyReadings;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Exceptions;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Domain.M200_Business.M205_Energy.EnergyReadings;

namespace SWFC.Application.M200_Business.M205_Energy.Analysis;

public sealed record GetEnergyAnalysisQuery(Guid MeterId);

public sealed class GetEnergyAnalysisValidator : ICommandValidator<GetEnergyAnalysisQuery>
{
    public Task<ValidationResult> ValidateAsync(
        GetEnergyAnalysisQuery command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.MeterId == Guid.Empty)
        {
            result.Add("M205.Analysis.Meter.Required", "Meter id is required.");
        }

        return Task.FromResult(result);
    }
}

public sealed class GetEnergyAnalysisPolicy : IAuthorizationPolicy<GetEnergyAnalysisQuery>
{
    public AuthorizationRequirement GetRequirement(GetEnergyAnalysisQuery request)
    {
        return new AuthorizationRequirement(
            requiredPermissions: new[] { "m205.energy-readings.read" });
    }
}

public sealed class GetEnergyAnalysisHandler
    : IUseCaseHandler<GetEnergyAnalysisQuery, EnergyAnalysisResultDto>
{
    private readonly IEnergyMeterReadRepository _meterReadRepository;
    private readonly IEnergyReadingReadRepository _readingReadRepository;

    public GetEnergyAnalysisHandler(
        IEnergyMeterReadRepository meterReadRepository,
        IEnergyReadingReadRepository readingReadRepository)
    {
        _meterReadRepository = meterReadRepository;
        _readingReadRepository = readingReadRepository;
    }

    public async Task<Result<EnergyAnalysisResultDto>> HandleAsync(
        GetEnergyAnalysisQuery request,
        CancellationToken cancellationToken = default)
    {
        var meter = await _meterReadRepository.GetByIdAsync(request.MeterId, cancellationToken);

        if (meter is null)
        {
            throw new NotFoundException($"Energy meter '{request.MeterId}' was not found.");
        }

        var readings = await _readingReadRepository.GetByMeterIdAsync(request.MeterId, cancellationToken);

        var orderedReadings = readings
            .OrderBy(x => x.Date.Value)
            .ThenBy(x => x.AuditInfo.CreatedAtUtc)
            .ToList();

        var readingDtos = new List<EnergyAnalysisReadingDto>(orderedReadings.Count);
        SWFC.Domain.M200_Business.M205_Energy.EnergyReadings.EnergyReading? previousReading = null;

        foreach (var reading in orderedReadings)
        {
            decimal? consumption = null;

            if (previousReading is not null)
            {
                var difference = reading.Value.Value - previousReading.Value.Value;
                consumption = difference >= 0 ? difference : null;
            }

            readingDtos.Add(new EnergyAnalysisReadingDto(
                reading.Id,
                reading.Date.Value,
                reading.Value.Value,
                reading.Source,
                consumption,
                reading.IsPlausibilityFlagged,
                reading.PlausibilityNote?.Value,
                reading.RfidExceptionReason?.Value,
                reading.OfflineCaptureId,
                reading.AuditInfo.CreatedAtUtc,
                reading.AuditInfo.CreatedBy));

            previousReading = reading;
        }

        var monthlyDtos = orderedReadings
            .GroupBy(x => new { x.Date.Value.Year, x.Date.Value.Month })
            .OrderBy(x => x.Key.Year)
            .ThenBy(x => x.Key.Month)
            .Select(group =>
            {
                var orderedGroup = group
                    .OrderBy(x => x.Date.Value)
                    .ThenBy(x => x.AuditInfo.CreatedAtUtc)
                    .ToList();

                var first = orderedGroup.First();
                var last = orderedGroup.Last();

                var consumption = last.Value.Value - first.Value.Value;
                if (consumption < 0)
                {
                    consumption = 0;
                }

                return new EnergyAnalysisMonthlyDto(
                    group.Key.Year,
                    group.Key.Month,
                    first.Date.Value,
                    last.Date.Value,
                    first.Value.Value,
                    last.Value.Value,
                    consumption,
                    orderedGroup.Count);
            })
            .ToList();

        var meterDto = new EnergyAnalysisMeterDto(
            meter.Id,
            meter.Name.Value,
            meter.MediumType,
            meter.MediumName.Value,
            meter.Unit.Value,
            meter.IsManualEntryEnabled,
            meter.IsExternalImportEnabled,
            meter.ExternalSystem?.Value,
            meter.ParentMeterId,
            meter.MachineId,
            meter.IsActive);

        var captureComparisons = orderedReadings
            .GroupBy(x => x.Date.Value.Date)
            .Select(group =>
            {
                var manual = group.FirstOrDefault(x => x.Source == EnergyReadingSource.Manual);
                var automatic = group.FirstOrDefault(x => x.Source is EnergyReadingSource.Automatic or EnergyReadingSource.Import or EnergyReadingSource.Realtime);

                return manual is null || automatic is null
                    ? null
                    : new EnergyAnalysisCaptureComparisonDto(
                        group.Key,
                        manual.Value.Value,
                        automatic.Value.Value,
                        Math.Abs(manual.Value.Value - automatic.Value.Value));
            })
            .Where(x => x is not null)
            .Select(x => x!)
            .ToList();

        var result = new EnergyAnalysisResultDto(
            meterDto,
            readingDtos,
            monthlyDtos,
            captureComparisons);

        return Result<EnergyAnalysisResultDto>.Success(result);
    }
}
