using SWFC.Application.M200_Business.M211_Analytics;
using SWFC.Application.M900_Intelligence.M901_Analytics_Intelligence_Engine;
using SWFC.Application.M900_Intelligence.M902_Prediction_Forecasting;
using SWFC.Application.M900_Intelligence.M903_Optimization;
using SWFC.Application.M900_Intelligence.M904_Anomaly_Detection;
using SWFC.Application.M900_Intelligence.M905_Intelligence_Governance;
using SWFC.Domain.M200_Business.M211_Analytics;

namespace SWFC.Application.M900_Intelligence;

public sealed record IntelligenceWorkspaceResult(
    IntelligenceAnalysisResult Analysis,
    IReadOnlyList<IntelligenceForecastResult> Forecasts,
    IReadOnlyList<IntelligenceOptimizationProposal> OptimizationProposals,
    IntelligenceAnomalyResult Anomalies,
    IntelligenceGovernanceReview Governance);

public sealed class IntelligenceWorkspaceService
{
    private readonly AnalyticsIntelligenceEngine _analytics;
    private readonly PredictionForecastingService _forecasting;
    private readonly OptimizationService _optimization;
    private readonly AnomalyDetectionService _anomalyDetection;
    private readonly IntelligenceGovernanceService _governance;

    public IntelligenceWorkspaceService()
        : this(
            new AnalyticsIntelligenceEngine(),
            new PredictionForecastingService(),
            new OptimizationService(),
            new AnomalyDetectionService(),
            new IntelligenceGovernanceService())
    {
    }

    public IntelligenceWorkspaceService(
        AnalyticsIntelligenceEngine analytics,
        PredictionForecastingService forecasting,
        OptimizationService optimization,
        AnomalyDetectionService anomalyDetection,
        IntelligenceGovernanceService governance)
    {
        _analytics = analytics;
        _forecasting = forecasting;
        _optimization = optimization;
        _anomalyDetection = anomalyDetection;
        _governance = governance;
    }

    public IntelligenceWorkspaceResult Evaluate(
        IntelligenceDataSet dataSet,
        IReadOnlyList<IntelligenceForecastRequest> forecastRequests)
    {
        ArgumentNullException.ThrowIfNull(dataSet);
        ArgumentNullException.ThrowIfNull(forecastRequests);

        var analysis = _analytics.Analyze(dataSet);
        var forecasts = forecastRequests
            .OrderBy(x => x.SourceModule, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.MetricCode, StringComparer.OrdinalIgnoreCase)
            .Select(_forecasting.Forecast)
            .ToList();
        var anomalies = _anomalyDetection.Detect(dataSet.Metrics);
        var proposals = _optimization.GenerateProposals(dataSet, forecasts, anomalies.Anomalies);

        var governance = _governance.Review(new IntelligenceGovernanceRequest(
            "M900.WorkspaceEvaluation",
            dataSet.SourceModules,
            analysis.Evidence
                .Concat(forecasts.Select(x => x.Evidence))
                .Concat(anomalies.Evidence)
                .Concat(proposals.Select(x => x.Evidence))
                .ToList(),
            ProposesAutomaticAction: proposals.Any(x => x.AutoExecutionEnabled)));

        return new IntelligenceWorkspaceResult(
            analysis,
            forecasts,
            proposals,
            anomalies,
            governance);
    }
}

public static class IntelligenceDataCollector
{
    public static IntelligenceDataSet FromAnalyticsSnapshot(
        AnalyticsSourceSnapshot snapshot,
        DateTimeOffset timestamp)
    {
        var metrics = new[]
        {
            new IntelligenceMetricObservation("M205", "ENERGY_CONSUMPTION", "Energy consumption", timestamp, snapshot.EnergyConsumption, "kWh"),
            new IntelligenceMetricObservation("M202", "OPEN_MAINTENANCE_ORDERS", "Open maintenance orders", timestamp, snapshot.OpenMaintenanceOrders, "count"),
            new IntelligenceMetricObservation("M207", "OPEN_QUALITY_CASES", "Open quality cases", timestamp, snapshot.OpenQualityCases, "count"),
            new IntelligenceMetricObservation("M204", "INVENTORY_BELOW_MINIMUM", "Inventory below minimum", timestamp, snapshot.InventoryBelowMinimum, "count"),
            new IntelligenceMetricObservation("M209", "OPEN_PROJECT_MEASURES", "Open project measures", timestamp, snapshot.OpenProjectMeasures, "count")
        };

        return new IntelligenceDataSet(metrics, []);
    }
}
