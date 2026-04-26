namespace SWFC.Application.M900_Intelligence;

public enum IntelligenceFindingSeverity
{
    Info,
    Notice,
    Warning,
    Critical
}

public enum IntelligenceReviewStatus
{
    ReviewRequired,
    ApprovedForReview,
    Blocked
}

public sealed record IntelligenceMetricObservation(
    string SourceModule,
    string MetricCode,
    string Label,
    DateTimeOffset Timestamp,
    decimal Value,
    string Unit,
    IReadOnlyDictionary<string, string>? Dimensions = null);

public sealed record IntelligenceEventObservation(
    string SourceModule,
    string EventCode,
    string Label,
    DateTimeOffset Timestamp,
    string EntityId,
    decimal? Magnitude = null,
    IReadOnlyDictionary<string, string>? Dimensions = null);

public sealed record IntelligenceDataSet(
    IReadOnlyList<IntelligenceMetricObservation> Metrics,
    IReadOnlyList<IntelligenceEventObservation> Events)
{
    public bool HasData => Metrics.Count > 0 || Events.Count > 0;

    public IReadOnlyList<string> SourceModules => Metrics
        .Select(x => x.SourceModule)
        .Concat(Events.Select(x => x.SourceModule))
        .Where(x => !string.IsNullOrWhiteSpace(x))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .Order(StringComparer.OrdinalIgnoreCase)
        .ToList();
}

public sealed record IntelligenceEvidence(
    string DataBasis,
    string Calculation,
    string Result,
    string Control);

public sealed record IntelligenceFinding(
    string Code,
    string Module,
    IntelligenceFindingSeverity Severity,
    string Title,
    decimal? Score,
    string? Unit,
    IntelligenceEvidence Evidence,
    bool RequiresApproval,
    string? ReviewTarget);

public sealed record IntelligenceCorrelation(
    string LeftMetricCode,
    string RightMetricCode,
    string LeftSourceModule,
    string RightSourceModule,
    decimal Coefficient,
    int SampleCount,
    IntelligenceEvidence Evidence);

public sealed record IntelligencePattern(
    string MetricCode,
    string SourceModule,
    string Direction,
    decimal FirstValue,
    decimal LastValue,
    decimal ChangePercent,
    int SampleCount,
    IntelligenceEvidence Evidence);

public sealed record IntelligenceAnalysisResult(
    IReadOnlyList<string> SourceModules,
    IReadOnlyList<IntelligenceCorrelation> Correlations,
    IReadOnlyList<IntelligencePattern> Patterns,
    IReadOnlyList<IntelligenceFinding> Findings,
    IReadOnlyList<IntelligenceEvidence> Evidence);

public sealed record IntelligenceSeriesPoint(DateTimeOffset Timestamp, decimal Value);

public sealed record IntelligenceForecastRequest(
    string SourceModule,
    string MetricCode,
    string Label,
    string Unit,
    IReadOnlyList<IntelligenceSeriesPoint> Points,
    int HorizonPeriods);

public sealed record IntelligenceForecastPoint(
    int Period,
    DateTimeOffset Timestamp,
    decimal ExpectedValue,
    decimal LowerBound,
    decimal UpperBound);

public sealed record IntelligenceForecastResult(
    string SourceModule,
    string MetricCode,
    string Label,
    string Unit,
    decimal TrendPerPeriod,
    decimal MeanAbsoluteError,
    IReadOnlyList<IntelligenceForecastPoint> Points,
    IntelligenceEvidence Evidence,
    bool IsRecommendationOnly);

public sealed record IntelligenceOptimizationProposal(
    string Code,
    string SourceModule,
    string Title,
    decimal BaselineValue,
    decimal TargetValue,
    decimal ExpectedImpact,
    string Unit,
    IntelligenceEvidence Evidence,
    bool RequiresApproval,
    bool AutoExecutionEnabled);

public sealed record IntelligenceAnomaly(
    string Code,
    string SourceModule,
    string MetricCode,
    DateTimeOffset Timestamp,
    decimal ActualValue,
    decimal BaselineValue,
    decimal Deviation,
    decimal Score,
    string Unit,
    IntelligenceFindingSeverity Severity,
    IntelligenceEvidence Evidence,
    bool CanProposeAlert,
    bool AlertExecuted);

public sealed record IntelligenceAnomalyResult(
    IReadOnlyList<IntelligenceAnomaly> Anomalies,
    IReadOnlyList<IntelligenceEvidence> Evidence);

public sealed record IntelligenceGovernancePolicy(
    bool RequiresDataBasis,
    bool RequiresExplanation,
    bool AllowsAutomaticActions,
    int MinimumSourceModuleCount);

public sealed record IntelligenceGovernanceRequest(
    string SubjectCode,
    IReadOnlyList<string> SourceModules,
    IReadOnlyList<IntelligenceEvidence> Evidence,
    bool ProposesAutomaticAction);

public sealed record IntelligenceGovernanceReview(
    string SubjectCode,
    IntelligenceReviewStatus Status,
    IReadOnlyList<string> Violations,
    IntelligenceEvidence Evidence);
