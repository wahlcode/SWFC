using SWFC.Application.M900_Intelligence;

namespace SWFC.Application.M900_Intelligence.M904_Anomaly_Detection;

public sealed class AnomalyDetectionService
{
    private const int MinimumBaselineSamples = 3;
    private const decimal WarningScore = 2m;
    private const decimal CriticalScore = 3m;

    public IntelligenceAnomalyResult Detect(IReadOnlyList<IntelligenceMetricObservation> observations)
    {
        ArgumentNullException.ThrowIfNull(observations);

        var normalized = observations
            .Where(x => !string.IsNullOrWhiteSpace(x.SourceModule) && !string.IsNullOrWhiteSpace(x.MetricCode))
            .OrderBy(x => x.SourceModule, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.MetricCode, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => SerializeDimensions(x.Dimensions))
            .ThenBy(x => x.Timestamp)
            .ToArray();

        var anomalies = normalized
            .GroupBy(x => new MetricGroupKey(
                x.SourceModule,
                x.MetricCode,
                x.Unit,
                SerializeDimensions(x.Dimensions)))
            .SelectMany(group => DetectGroup(group.Key, group.OrderBy(x => x.Timestamp).ToArray()))
            .OrderBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var evidence = new IntelligenceEvidence(
            $"Observations={normalized.Length}; MinimumBaselineSamples={MinimumBaselineSamples}",
            "Latest value per metric group is compared with the previous baseline using absolute z-score.",
            $"Anomalies={anomalies.Count}",
            "M904 may propose an alert target for M303, but AlertExecuted is always false.");

        return new IntelligenceAnomalyResult(anomalies, [evidence]);
    }

    private static IEnumerable<IntelligenceAnomaly> DetectGroup(
        MetricGroupKey key,
        IReadOnlyList<IntelligenceMetricObservation> ordered)
    {
        if (ordered.Count <= MinimumBaselineSamples)
        {
            yield break;
        }

        var latest = ordered[^1];
        var baselineValues = ordered.Take(ordered.Count - 1).Select(x => x.Value).ToArray();
        var baselineAverage = baselineValues.Average();
        var standardDeviation = CalculatePopulationStandardDeviation(baselineValues, baselineAverage);
        var deviation = latest.Value - baselineAverage;
        var score = standardDeviation == 0
            ? (deviation == 0 ? 0 : CriticalScore)
            : Math.Abs(deviation) / standardDeviation;

        if (score < WarningScore)
        {
            yield break;
        }

        var roundedScore = Math.Round(score, 2, MidpointRounding.AwayFromZero);
        var severity = roundedScore >= CriticalScore
            ? IntelligenceFindingSeverity.Critical
            : IntelligenceFindingSeverity.Warning;

        yield return new IntelligenceAnomaly(
            $"M904.{key.SourceModule}.{key.MetricCode}.{latest.Timestamp.UtcDateTime:yyyyMMddHHmmss}",
            key.SourceModule,
            key.MetricCode,
            latest.Timestamp,
            latest.Value,
            Math.Round(baselineAverage, 2, MidpointRounding.AwayFromZero),
            Math.Round(deviation, 2, MidpointRounding.AwayFromZero),
            roundedScore,
            key.Unit,
            severity,
            new IntelligenceEvidence(
                $"BaselineSamples={baselineValues.Length}; Latest={latest.Value}; BaselineAverage={Math.Round(baselineAverage, 2, MidpointRounding.AwayFromZero)}",
                "Absolute deviation divided by population standard deviation of prior values; zero variance with changed latest value is critical.",
                $"Score={roundedScore}; Severity={severity}",
                "Anomaly is explainable and only proposes review or alert handoff."),
            CanProposeAlert: true,
            AlertExecuted: false);
    }

    private static decimal CalculatePopulationStandardDeviation(
        IReadOnlyList<decimal> values,
        decimal average)
    {
        if (values.Count == 0)
        {
            return 0;
        }

        var variance = values.Sum(value => (value - average) * (value - average)) / values.Count;
        return (decimal)Math.Sqrt((double)variance);
    }

    private static string SerializeDimensions(IReadOnlyDictionary<string, string>? dimensions)
    {
        if (dimensions is null || dimensions.Count == 0)
        {
            return string.Empty;
        }

        return string.Join(
            "|",
            dimensions
                .OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
                .Select(x => $"{x.Key}={x.Value}"));
    }

    private sealed record MetricGroupKey(
        string SourceModule,
        string MetricCode,
        string Unit,
        string Dimensions);
}
