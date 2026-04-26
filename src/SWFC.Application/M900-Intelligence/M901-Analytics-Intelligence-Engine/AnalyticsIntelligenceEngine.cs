using SWFC.Application.M900_Intelligence;

namespace SWFC.Application.M900_Intelligence.M901_Analytics_Intelligence_Engine;

public sealed class AnalyticsIntelligenceEngine
{
    private const decimal StrongCorrelationThreshold = 0.75m;
    private const decimal PatternChangeThresholdPercent = 10m;

    public IntelligenceAnalysisResult Analyze(IntelligenceDataSet dataSet)
    {
        ArgumentNullException.ThrowIfNull(dataSet);

        var metrics = dataSet.Metrics
            .Where(x => !string.IsNullOrWhiteSpace(x.SourceModule) && !string.IsNullOrWhiteSpace(x.MetricCode))
            .OrderBy(x => x.SourceModule, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.MetricCode, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.Timestamp)
            .ToList();

        var correlations = CalculateCorrelations(metrics);
        var patterns = CalculatePatterns(metrics);
        var findings = BuildFindings(correlations, patterns);

        var evidence = new IntelligenceEvidence(
            $"Metrics={metrics.Count}; Events={dataSet.Events.Count}; Sources={string.Join(",", dataSet.SourceModules)}",
            "Metrics are sorted by source, code and timestamp; correlations use Pearson on shared UTC dates; patterns compare first and last value per metric.",
            $"Correlations={correlations.Count}; Patterns={patterns.Count}; Findings={findings.Count}",
            "M901 returns findings only. It does not execute process control, runtime commands or data changes.");

        return new IntelligenceAnalysisResult(
            dataSet.SourceModules,
            correlations,
            patterns,
            findings,
            [evidence]);
    }

    private static IReadOnlyList<IntelligenceCorrelation> CalculateCorrelations(
        IReadOnlyList<IntelligenceMetricObservation> metrics)
    {
        var series = metrics
            .GroupBy(x => new MetricKey(x.SourceModule, x.MetricCode, x.Unit))
            .OrderBy(x => x.Key.SourceModule, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.Key.MetricCode, StringComparer.OrdinalIgnoreCase)
            .Select(group => new
            {
                group.Key,
                Points = group
                    .GroupBy(x => x.Timestamp.UtcDateTime.Date)
                    .ToDictionary(
                        x => x.Key,
                        x => x.OrderBy(y => y.Timestamp).Last().Value)
            })
            .ToArray();

        var correlations = new List<IntelligenceCorrelation>();

        for (var leftIndex = 0; leftIndex < series.Length; leftIndex++)
        {
            for (var rightIndex = leftIndex + 1; rightIndex < series.Length; rightIndex++)
            {
                var left = series[leftIndex];
                var right = series[rightIndex];

                if (string.Equals(left.Key.SourceModule, right.Key.SourceModule, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(left.Key.MetricCode, right.Key.MetricCode, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var sharedDates = left.Points.Keys
                    .Intersect(right.Points.Keys)
                    .Order()
                    .ToArray();

                if (sharedDates.Length < 2)
                {
                    continue;
                }

                var leftValues = sharedDates.Select(date => left.Points[date]).ToArray();
                var rightValues = sharedDates.Select(date => right.Points[date]).ToArray();
                var coefficient = CalculatePearson(leftValues, rightValues);

                correlations.Add(new IntelligenceCorrelation(
                    left.Key.MetricCode,
                    right.Key.MetricCode,
                    left.Key.SourceModule,
                    right.Key.SourceModule,
                    Math.Round(coefficient, 4, MidpointRounding.AwayFromZero),
                    sharedDates.Length,
                    new IntelligenceEvidence(
                        $"SharedDates={sharedDates.Length}; Left={left.Key.SourceModule}.{left.Key.MetricCode}; Right={right.Key.SourceModule}.{right.Key.MetricCode}",
                        "Pearson correlation over daily last values.",
                        $"Coefficient={Math.Round(coefficient, 4, MidpointRounding.AwayFromZero)}",
                        "Correlation is explanatory and is not an action trigger.")));
            }
        }

        return correlations;
    }

    private static IReadOnlyList<IntelligencePattern> CalculatePatterns(
        IReadOnlyList<IntelligenceMetricObservation> metrics)
    {
        return metrics
            .GroupBy(x => new MetricKey(x.SourceModule, x.MetricCode, x.Unit))
            .OrderBy(x => x.Key.SourceModule, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.Key.MetricCode, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var ordered = group
                    .OrderBy(x => x.Timestamp)
                    .ThenBy(x => x.Value)
                    .ToArray();

                if (ordered.Length < 2)
                {
                    return null;
                }

                var first = ordered.First().Value;
                var last = ordered.Last().Value;
                var changePercent = first == 0
                    ? (last == 0 ? 0 : 100)
                    : ((last - first) / Math.Abs(first)) * 100;
                var direction = changePercent > 0 ? "Rising" : changePercent < 0 ? "Falling" : "Stable";

                return new IntelligencePattern(
                    group.Key.MetricCode,
                    group.Key.SourceModule,
                    direction,
                    first,
                    last,
                    Math.Round(changePercent, 2, MidpointRounding.AwayFromZero),
                    ordered.Length,
                    new IntelligenceEvidence(
                        $"Samples={ordered.Length}; First={first}; Last={last}",
                        "Relative change between first and last sorted metric value.",
                        $"Direction={direction}; ChangePercent={Math.Round(changePercent, 2, MidpointRounding.AwayFromZero)}",
                        "Pattern is informational and requires review before operational use."));
            })
            .Where(x => x is not null)
            .Select(x => x!)
            .ToList();
    }

    private static IReadOnlyList<IntelligenceFinding> BuildFindings(
        IReadOnlyList<IntelligenceCorrelation> correlations,
        IReadOnlyList<IntelligencePattern> patterns)
    {
        var findings = new List<IntelligenceFinding>();

        findings.AddRange(correlations
            .Where(x => Math.Abs(x.Coefficient) >= StrongCorrelationThreshold)
            .Select(x => new IntelligenceFinding(
                $"M901.Correlation.{x.LeftSourceModule}.{x.LeftMetricCode}.{x.RightSourceModule}.{x.RightMetricCode}",
                "M901",
                IntelligenceFindingSeverity.Notice,
                $"Strong correlation between {x.LeftMetricCode} and {x.RightMetricCode}",
                x.Coefficient,
                null,
                x.Evidence,
                RequiresApproval: true,
                ReviewTarget: "Analytics review")));

        findings.AddRange(patterns
            .Where(x => Math.Abs(x.ChangePercent) >= PatternChangeThresholdPercent)
            .Select(x => new IntelligenceFinding(
                $"M901.Pattern.{x.SourceModule}.{x.MetricCode}",
                "M901",
                Math.Abs(x.ChangePercent) >= 25m ? IntelligenceFindingSeverity.Warning : IntelligenceFindingSeverity.Notice,
                $"{x.MetricCode} is {x.Direction.ToLowerInvariant()}",
                x.ChangePercent,
                "%",
                x.Evidence,
                RequiresApproval: true,
                ReviewTarget: "Pattern review")));

        return findings
            .OrderBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static decimal CalculatePearson(IReadOnlyList<decimal> leftValues, IReadOnlyList<decimal> rightValues)
    {
        var leftAverage = leftValues.Average();
        var rightAverage = rightValues.Average();
        decimal numerator = 0;
        decimal leftSquared = 0;
        decimal rightSquared = 0;

        for (var index = 0; index < leftValues.Count; index++)
        {
            var leftDeviation = leftValues[index] - leftAverage;
            var rightDeviation = rightValues[index] - rightAverage;

            numerator += leftDeviation * rightDeviation;
            leftSquared += leftDeviation * leftDeviation;
            rightSquared += rightDeviation * rightDeviation;
        }

        if (leftSquared == 0 || rightSquared == 0)
        {
            return 0;
        }

        return numerator / (decimal)Math.Sqrt((double)(leftSquared * rightSquared));
    }

    private sealed record MetricKey(string SourceModule, string MetricCode, string Unit);
}
