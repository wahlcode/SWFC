using SWFC.Domain.M200_Business.M205_Energy.EnergyMeters;
using SWFC.Domain.M200_Business.M205_Energy.EnergyReadings;

namespace SWFC.Application.M200_Business.M205_Energy.Analysis;

public sealed record EnergyAnalysisMeterDto(
    Guid MeterId,
    string MeterName,
    EnergyMediumType MediumType,
    string MediumName,
    string Unit,
    bool IsManualEntryEnabled,
    bool IsExternalImportEnabled,
    string? ExternalSystem,
    Guid? ParentMeterId,
    Guid? MachineId,
    bool IsActive);

public sealed record EnergyAnalysisReadingDto(
    Guid ReadingId,
    DateTime Date,
    decimal Value,
    EnergyReadingSource Source,
    decimal? ConsumptionSincePrevious,
    bool IsPlausibilityFlagged,
    string? PlausibilityNote,
    string? RfidExceptionReason,
    Guid? OfflineCaptureId,
    DateTime CreatedAtUtc,
    string CreatedBy);

public sealed record EnergyAnalysisCaptureComparisonDto(
    DateTime Date,
    decimal ManualValue,
    decimal AutomaticValue,
    decimal Difference);

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
    IReadOnlyList<EnergyAnalysisMonthlyDto> Monthly,
    IReadOnlyList<EnergyAnalysisCaptureComparisonDto> CaptureComparisons);
