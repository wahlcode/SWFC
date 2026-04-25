using SWFC.Domain.M200_Business.M205_Energy.EnergyMeters;
using SWFC.Domain.M200_Business.M205_Energy.EnergyReadings;

namespace SWFC.Application.M200_Business.M205_Energy.Analysis;

public sealed record EnergyAnalysisMeterDto(
    Guid MeterId,
    string MeterName,
    EnergyMediumType MediumType,
    string Unit,
    bool IsManualEntryEnabled,
    bool IsExternalImportEnabled,
    string? ExternalSystem,
    Guid? MachineId,
    bool IsActive);

public sealed record EnergyAnalysisReadingDto(
    Guid ReadingId,
    DateTime Date,
    decimal Value,
    EnergyReadingSource Source,
    decimal? ConsumptionSincePrevious,
    DateTime CreatedAtUtc,
    string CreatedBy);

public sealed record EnergyAnalysisMonthlyDto(
    int Year,
    int Month,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    decimal StartValue,
    decimal EndValue,
    decimal Consumption,
    int ReadingCount);

public sealed record EnergyAnalysisResultDto(
    EnergyAnalysisMeterDto Meter,
    IReadOnlyList<EnergyAnalysisReadingDto> Readings,
    IReadOnlyList<EnergyAnalysisMonthlyDto> Monthly);
