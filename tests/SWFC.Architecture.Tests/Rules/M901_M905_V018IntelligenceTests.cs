using System.Text.Json;
using SWFC.Application.M900_Intelligence;
using SWFC.Application.M900_Intelligence.M901_Analytics_Intelligence_Engine;
using SWFC.Application.M900_Intelligence.M902_Prediction_Forecasting;
using SWFC.Application.M900_Intelligence.M903_Optimization;
using SWFC.Application.M900_Intelligence.M904_Anomaly_Detection;
using SWFC.Application.M900_Intelligence.M905_Intelligence_Governance;
using SWFC.Architecture.Tests.Support;

namespace SWFC.Architecture.Tests.Rules;

public sealed class M901_M905_V018IntelligenceTests
{
    [Fact]
    public void V018_Roadmap_And_Intelligence_Modules_Should_Be_Marked_Done_After_Verification()
    {
        using var roadmap = JsonDocument.Parse(File.ReadAllText(
            RepositoryRoot.Combine("docs", "M600-Planning", "M601-Roadmap", "roadmap.json")));
        using var modules = JsonDocument.Parse(File.ReadAllText(
            RepositoryRoot.Combine("src", "SWFC.Web", "wwwroot", "data", "modules.json")));

        var v018 = roadmap.RootElement
            .GetProperty("Versions")
            .EnumerateArray()
            .Single(version => version.GetProperty("Version").GetString() == "v0.18.0");

        Assert.Equal("Done", v018.GetProperty("Status").GetString());
        Assert.All(
            new[] { "M901", "M902", "M903", "M904", "M905" },
            module => Assert.Contains(module, ReadStringArray(v018, "PrimaryModules")));
        Assert.All(
            v018.GetProperty("Milestones").EnumerateArray(),
            milestone => Assert.Equal("Done", milestone.GetProperty("Status").GetString()));

        foreach (var moduleCode in new[] { "M901", "M902", "M903", "M904", "M905" })
        {
            var module = modules.RootElement
                .GetProperty("Groups")
                .EnumerateArray()
                .SelectMany(group => group.GetProperty("Modules").EnumerateArray())
                .Single(item => item.GetProperty("Code").GetString() == moduleCode);

            Assert.Equal("Full Complete", module.GetProperty("Status").GetString());
            Assert.Equal(100, module.GetProperty("ProgressPercent").GetInt32());
            Assert.All(
                module.GetProperty("WorkItems").EnumerateArray(),
                workItem => Assert.Equal("Done", workItem.GetProperty("Status").GetString()));
        }
    }

    [Fact]
    public void M901_Should_Calculate_Reproducible_Correlations_And_Explain_Findings()
    {
        var engine = new AnalyticsIntelligenceEngine();
        var dataSet = BuildDataSet();

        var first = engine.Analyze(dataSet);
        var second = engine.Analyze(dataSet);

        var energyProduction = first.Correlations.Single(x =>
            x.LeftMetricCode == "ENERGY_CONSUMPTION" &&
            x.RightMetricCode == "PRODUCTION_OUTPUT");

        Assert.Equal(first.Correlations.Select(x => x.Coefficient), second.Correlations.Select(x => x.Coefficient));
        Assert.True(energyProduction.Coefficient > 0.95m);
        Assert.Contains(first.Patterns, x => x.MetricCode == "SCRAP_QUANTITY" && x.Direction == "Rising");
        Assert.All(first.Findings, finding => Assert.True(finding.RequiresApproval));
        Assert.All(first.Findings, finding => Assert.NotNull(finding.Evidence.DataBasis));
    }

    [Fact]
    public void M902_Should_Forecast_Deterministically_With_Uncertainty_And_No_Decision()
    {
        var service = new PredictionForecastingService();
        var request = new IntelligenceForecastRequest(
            "M205",
            "ENERGY_CONSUMPTION",
            "Energy consumption",
            "kWh",
            [
                new(new DateTimeOffset(2026, 4, 20, 0, 0, 0, TimeSpan.Zero), 100),
                new(new DateTimeOffset(2026, 4, 21, 0, 0, 0, TimeSpan.Zero), 110),
                new(new DateTimeOffset(2026, 4, 22, 0, 0, 0, TimeSpan.Zero), 121),
                new(new DateTimeOffset(2026, 4, 23, 0, 0, 0, TimeSpan.Zero), 130)
            ],
            HorizonPeriods: 2);

        var first = service.Forecast(request);
        var second = service.Forecast(request);

        Assert.Equal(first.Points.Select(x => x.ExpectedValue), second.Points.Select(x => x.ExpectedValue));
        Assert.Equal(10.1m, first.TrendPerPeriod);
        Assert.True(first.MeanAbsoluteError > 0);
        Assert.True(first.IsRecommendationOnly);
        Assert.All(first.Points, point => Assert.True(point.LowerBound <= point.ExpectedValue && point.UpperBound >= point.ExpectedValue));
    }

    [Fact]
    public void M903_Should_Create_Approval_Bound_Optimization_Proposals_From_Data()
    {
        var optimization = new OptimizationService();
        var anomaly = new IntelligenceAnomaly(
            "M904.M205.ENERGY_CONSUMPTION.20260424000000",
            "M205",
            "ENERGY_CONSUMPTION",
            new DateTimeOffset(2026, 4, 24, 0, 0, 0, TimeSpan.Zero),
            190,
            120,
            70,
            3.5m,
            "kWh",
            IntelligenceFindingSeverity.Critical,
            new IntelligenceEvidence("sample", "z-score", "critical", "review only"),
            CanProposeAlert: true,
            AlertExecuted: false);

        var proposals = optimization.GenerateProposals(BuildDataSet(), [], [anomaly]);

        Assert.Contains(proposals, x => x.Code == "M903.Energy.EfficiencyReview");
        Assert.Contains(proposals, x => x.Code.StartsWith("M903.AnomalyReview.", StringComparison.Ordinal));
        Assert.All(proposals, proposal => Assert.True(proposal.RequiresApproval));
        Assert.All(proposals, proposal => Assert.False(proposal.AutoExecutionEnabled));
        Assert.All(proposals, proposal => Assert.NotEmpty(proposal.Evidence.Calculation));
    }

    [Fact]
    public void M904_Should_Detect_Explainable_Anomalies_Without_Executing_Alerts()
    {
        var service = new AnomalyDetectionService();
        var observations = new[]
        {
            Observation("M205", "ENERGY_CONSUMPTION", "Energy", "kWh", 20, 100),
            Observation("M205", "ENERGY_CONSUMPTION", "Energy", "kWh", 21, 102),
            Observation("M205", "ENERGY_CONSUMPTION", "Energy", "kWh", 22, 98),
            Observation("M205", "ENERGY_CONSUMPTION", "Energy", "kWh", 23, 101),
            Observation("M205", "ENERGY_CONSUMPTION", "Energy", "kWh", 24, 150)
        };

        var result = service.Detect(observations);
        var anomaly = Assert.Single(result.Anomalies);

        Assert.Equal(IntelligenceFindingSeverity.Critical, anomaly.Severity);
        Assert.True(anomaly.CanProposeAlert);
        Assert.False(anomaly.AlertExecuted);
        Assert.Contains("BaselineSamples=4", anomaly.Evidence.DataBasis, StringComparison.Ordinal);
    }

    [Fact]
    public void M905_Should_Block_Unexplained_Or_Automatic_Intelligence_Actions()
    {
        var service = new IntelligenceGovernanceService();
        var allowed = service.Review(new IntelligenceGovernanceRequest(
            "M901.Analysis",
            ["M205", "M207"],
            [new IntelligenceEvidence("data", "calc", "result", "control")],
            ProposesAutomaticAction: false));
        var blocked = service.Review(new IntelligenceGovernanceRequest(
            "M903.AutoAction",
            ["M205"],
            [],
            ProposesAutomaticAction: true));

        Assert.Equal(IntelligenceReviewStatus.ApprovedForReview, allowed.Status);
        Assert.Equal(IntelligenceReviewStatus.Blocked, blocked.Status);
        Assert.Contains(blocked.Violations, x => x.Contains("Automatic actions", StringComparison.Ordinal));
        Assert.Contains(blocked.Violations, x => x.Contains("explainability", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void M900_Intelligence_Should_Not_Contain_Runtime_Actions_Db_Access_Or_Placeholders()
    {
        var files = RepositoryRoot.EnumerateFiles(
                Path.Combine("src", "SWFC.Application", "M900-Intelligence"),
                "*.cs",
                includeGeneratedFiles: false)
            .ToArray();

        Assert.NotEmpty(files);

        var combined = string.Join(Environment.NewLine, files.Select(File.ReadAllText));
        Assert.DoesNotContain("TODO", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Placeholder", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("DbContext", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("UseNpgsql", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("SaveChanges", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("HttpClient", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("ControlCommand", combined, StringComparison.Ordinal);
    }

    private static IntelligenceDataSet BuildDataSet()
    {
        return new IntelligenceDataSet(
            [
                Observation("M205", "ENERGY_CONSUMPTION", "Energy", "kWh", 20, 100),
                Observation("M205", "ENERGY_CONSUMPTION", "Energy", "kWh", 21, 110),
                Observation("M205", "ENERGY_CONSUMPTION", "Energy", "kWh", 22, 120),
                Observation("M205", "ENERGY_CONSUMPTION", "Energy", "kWh", 23, 130),
                Observation("M205", "ENERGY_CONSUMPTION", "Energy", "kWh", 24, 170),
                Observation("M212", "PRODUCTION_OUTPUT", "Production", "pcs", 20, 1000),
                Observation("M212", "PRODUCTION_OUTPUT", "Production", "pcs", 21, 1100),
                Observation("M212", "PRODUCTION_OUTPUT", "Production", "pcs", 22, 1200),
                Observation("M212", "PRODUCTION_OUTPUT", "Production", "pcs", 23, 1300),
                Observation("M212", "PRODUCTION_OUTPUT", "Production", "pcs", 24, 1500),
                Observation("M207", "SCRAP_QUANTITY", "Scrap", "pcs", 20, 2),
                Observation("M207", "SCRAP_QUANTITY", "Scrap", "pcs", 21, 3),
                Observation("M207", "SCRAP_QUANTITY", "Scrap", "pcs", 22, 5),
                Observation("M207", "SCRAP_QUANTITY", "Scrap", "pcs", 23, 8)
            ],
            [
                new IntelligenceEventObservation(
                    "M202",
                    "MAINTENANCE_ORDER_OPENED",
                    "Maintenance order opened",
                    new DateTimeOffset(2026, 4, 24, 8, 0, 0, TimeSpan.Zero),
                    "MO-1")
            ]);
    }

    private static IntelligenceMetricObservation Observation(
        string sourceModule,
        string metricCode,
        string label,
        string unit,
        int day,
        decimal value)
    {
        return new IntelligenceMetricObservation(
            sourceModule,
            metricCode,
            label,
            new DateTimeOffset(2026, 4, day, 0, 0, 0, TimeSpan.Zero),
            value,
            unit);
    }

    private static IReadOnlyCollection<string> ReadStringArray(JsonElement element, string propertyName) =>
        element
            .GetProperty(propertyName)
            .EnumerateArray()
            .Select(x => x.GetString() ?? string.Empty)
            .ToArray();
}
