using SWFC.Application.M900_Intelligence;

namespace SWFC.Application.M900_Intelligence.M903_Optimization;

public sealed class OptimizationService
{
    private const decimal EnergyEfficiencyIncreaseThresholdPercent = 10m;

    public IReadOnlyList<IntelligenceOptimizationProposal> GenerateProposals(
        IntelligenceDataSet dataSet,
        IReadOnlyList<IntelligenceForecastResult> forecasts,
        IReadOnlyList<IntelligenceAnomaly> anomalies)
    {
        ArgumentNullException.ThrowIfNull(dataSet);
        ArgumentNullException.ThrowIfNull(forecasts);
        ArgumentNullException.ThrowIfNull(anomalies);

        var proposals = new List<IntelligenceOptimizationProposal>();

        var energyProposal = BuildEnergyEfficiencyProposal(dataSet);
        if (energyProposal is not null)
        {
            proposals.Add(energyProposal);
        }

        proposals.AddRange(forecasts
            .Where(x => x.TrendPerPeriod > 0 && IsMaintenanceMetric(x.MetricCode))
            .Select(x => new IntelligenceOptimizationProposal(
                $"M903.Maintenance.{x.SourceModule}.{x.MetricCode}",
                x.SourceModule,
                "Review maintenance capacity before demand increases",
                BaselineValue: x.Points.FirstOrDefault()?.ExpectedValue ?? 0,
                TargetValue: x.Points.LastOrDefault()?.ExpectedValue ?? 0,
                ExpectedImpact: Math.Round(x.TrendPerPeriod * x.Points.Count, 2, MidpointRounding.AwayFromZero),
                x.Unit,
                new IntelligenceEvidence(
                    $"Forecast={x.MetricCode}; Periods={x.Points.Count}",
                    "Positive deterministic forecast trend is converted into a review proposal.",
                    $"ExpectedIncrease={Math.Round(x.TrendPerPeriod * x.Points.Count, 2, MidpointRounding.AwayFromZero)} {x.Unit}",
                    "M903 proposal requires approval and has no automatic execution."),
                RequiresApproval: true,
                AutoExecutionEnabled: false)));

        proposals.AddRange(anomalies
            .Where(x => x.Severity is IntelligenceFindingSeverity.Warning or IntelligenceFindingSeverity.Critical)
            .Select(x => new IntelligenceOptimizationProposal(
                $"M903.AnomalyReview.{x.SourceModule}.{x.MetricCode}",
                x.SourceModule,
                $"Review optimization potential for anomalous {x.MetricCode}",
                x.BaselineValue,
                x.ActualValue,
                x.Deviation,
                x.Unit,
                new IntelligenceEvidence(
                    $"Anomaly={x.Code}; Score={x.Score}",
                    "Anomaly evidence is converted into a controlled review proposal.",
                    $"Deviation={x.Deviation} {x.Unit}",
                    "M903 does not execute corrective action; approval is mandatory."),
                RequiresApproval: true,
                AutoExecutionEnabled: false)));

        return proposals
            .GroupBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
            .Select(x => x.First())
            .OrderBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static IntelligenceOptimizationProposal? BuildEnergyEfficiencyProposal(IntelligenceDataSet dataSet)
    {
        var energyByDay = AggregateByDay(dataSet.Metrics, "M205", "ENERGY_CONSUMPTION");
        var productionByDay = AggregateByDay(dataSet.Metrics, "M212", "PRODUCTION_OUTPUT");
        var sharedDays = energyByDay.Keys.Intersect(productionByDay.Keys).Order().ToArray();

        if (sharedDays.Length < 3)
        {
            return null;
        }

        var ratios = sharedDays
            .Where(day => productionByDay[day] > 0)
            .Select(day => new
            {
                Day = day,
                Ratio = energyByDay[day] / productionByDay[day]
            })
            .ToArray();

        if (ratios.Length < 3)
        {
            return null;
        }

        var baseline = ratios.Take(ratios.Length - 1).Average(x => x.Ratio);
        var latest = ratios.Last().Ratio;

        if (baseline <= 0)
        {
            return null;
        }

        var increasePercent = ((latest - baseline) / baseline) * 100;
        if (increasePercent < EnergyEfficiencyIncreaseThresholdPercent)
        {
            return null;
        }

        var target = Math.Round(baseline, 4, MidpointRounding.AwayFromZero);
        return new IntelligenceOptimizationProposal(
            "M903.Energy.EfficiencyReview",
            "M205",
            "Review energy consumption per produced unit",
            Math.Round(latest, 4, MidpointRounding.AwayFromZero),
            target,
            Math.Round(latest - baseline, 4, MidpointRounding.AwayFromZero),
            "energy/unit",
            new IntelligenceEvidence(
                $"SharedDays={ratios.Length}; Energy=M205.ENERGY_CONSUMPTION; Production=M212.PRODUCTION_OUTPUT",
                "Energy per produced unit is compared against the prior daily average.",
                $"IncreasePercent={Math.Round(increasePercent, 2, MidpointRounding.AwayFromZero)}",
                "Proposal is review-only and cannot change machine or runtime settings."),
            RequiresApproval: true,
            AutoExecutionEnabled: false);
    }

    private static Dictionary<DateTime, decimal> AggregateByDay(
        IEnumerable<IntelligenceMetricObservation> metrics,
        string sourceModule,
        string metricCode)
    {
        return metrics
            .Where(x =>
                string.Equals(x.SourceModule, sourceModule, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.MetricCode, metricCode, StringComparison.OrdinalIgnoreCase))
            .GroupBy(x => x.Timestamp.UtcDateTime.Date)
            .ToDictionary(
                x => x.Key,
                x => x.Sum(item => item.Value));
    }

    private static bool IsMaintenanceMetric(string metricCode)
    {
        return metricCode.Contains("MAINTENANCE", StringComparison.OrdinalIgnoreCase) ||
            metricCode.Contains("DOWNTIME", StringComparison.OrdinalIgnoreCase);
    }
}
