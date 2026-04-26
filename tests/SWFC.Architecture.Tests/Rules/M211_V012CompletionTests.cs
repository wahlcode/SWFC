using System.Text.Json;
using SWFC.Architecture.Tests.Support;
using SWFC.Application.M200_Business.M211_Analytics;
using SWFC.Domain.M200_Business.M211_Analytics;

namespace SWFC.Architecture.Tests.Rules;

public sealed class M211_V012CompletionTests
{
    [Fact]
    public void V012_Roadmap_Should_Be_Marked_Done_With_All_Milestones_Done()
    {
        using var roadmap = JsonDocument.Parse(File.ReadAllText(
            RepositoryRoot.Combine("docs", "M600-Planning", "M601-Roadmap", "roadmap.json")));

        var version = roadmap.RootElement
            .GetProperty("Versions")
            .EnumerateArray()
            .Single(x => x.GetProperty("Version").GetString() == "v0.12.0");

        Assert.Equal("Done", version.GetProperty("Status").GetString());
        Assert.Contains("M211", ReadStringArray(version, "PrimaryModules"));
        Assert.All(
            new[] { "M201", "M202", "M204", "M205", "M207", "M302" },
            module => Assert.Contains(module, ReadStringArray(version, "RequiredModules")));
        Assert.All(
            version.GetProperty("Milestones").EnumerateArray(),
            milestone => Assert.Equal("Done", milestone.GetProperty("Status").GetString()));
    }

    [Fact]
    public void M211_WorkItems_Should_Be_Done_For_V012()
    {
        using var modules = JsonDocument.Parse(File.ReadAllText(
            RepositoryRoot.Combine("src", "SWFC.Web", "wwwroot", "data", "modules.json")));

        var m211 = modules.RootElement
            .GetProperty("Groups")
            .EnumerateArray()
            .SelectMany(group => group.GetProperty("Modules").EnumerateArray())
            .Single(module => module.GetProperty("Code").GetString() == "M211");

        Assert.Equal("Full Complete", m211.GetProperty("Status").GetString());
        Assert.Equal(100, m211.GetProperty("ProgressPercent").GetInt32());
        Assert.All(
            m211.GetProperty("WorkItems").EnumerateArray(),
            item => Assert.Equal("Done", item.GetProperty("Status").GetString()));
    }

    [Fact]
    public void M211_Analytics_Should_Calculate_Kpis_From_Source_Modules_Without_Owned_Facts()
    {
        var snapshot = new AnalyticsSourceSnapshot(
            EnergyConsumption: 100.125m,
            OpenMaintenanceOrders: 2,
            OpenQualityCases: 1,
            InventoryBelowMinimum: 3,
            OpenProjectMeasures: 4);

        var kpis = AnalyticsCalculator.CalculateOperationalKpis(snapshot);
        var energyKpi = kpis.Single(x => x.Code == "M205_ENERGY_TOTAL");
        var report = new ReportDefinition(
            "OPS",
            "Operative Uebersicht",
            "Table",
            ["M202", "M204", "M205", "M207"],
            ["PDF", "Excel"]);

        Assert.Equal(100.13m, energyKpi.Value);
        Assert.All(kpis, kpi => Assert.NotEqual("M211", kpi.SourceModule));
        Assert.True(report.HasNoOwnedFacts);
    }

    [Fact]
    public void M211_Integration_Should_Have_Service_Ui_Exports_And_No_Double_Data_Holding()
    {
        var service = new AnalyticsWorkspaceService();
        var overview = service.GetOverview();

        Assert.NotEmpty(overview.Kpis);
        Assert.Contains(overview.Dashboards, x => x.IsDepartmentSpecific);
        Assert.Contains(overview.Reports, x => x.ExportFormats.Contains("PDF"));
        Assert.Contains(overview.Reports, x => x.ExportFormats.Contains("Excel"));
        Assert.All(overview.Reports, report => Assert.True(report.HasNoOwnedFacts));

        var requiredFiles = new[]
        {
            RepositoryRoot.Combine("src", "SWFC.Domain", "M200-Business", "M211-Analytics", "AnalyticsModels.cs"),
            RepositoryRoot.Combine("src", "SWFC.Application", "M200-Business", "M211-Analytics", "AnalyticsWorkspaceService.cs"),
            RepositoryRoot.Combine("src", "SWFC.Web", "Pages", "M200-Business", "M211-Analytics", "Analytics.razor"),
            RepositoryRoot.Combine("src", "SWFC.Web", "Program.cs"),
            RepositoryRoot.Combine("src", "SWFC.Web", "wwwroot", "data", "sidebar.json")
        };

        Assert.All(requiredFiles, file => Assert.True(File.Exists(file), file));

        var combinedContent = string.Join(Environment.NewLine, requiredFiles.Select(File.ReadAllText));
        Assert.Contains("/m200/analytics", combinedContent, StringComparison.Ordinal);
        Assert.Contains("AddSingleton<AnalyticsWorkspaceService>", combinedContent, StringComparison.Ordinal);
        Assert.Contains("M205_ENERGY_TOTAL", combinedContent, StringComparison.Ordinal);
        Assert.Contains("\"PDF\"", combinedContent, StringComparison.Ordinal);
        Assert.Contains("\"Excel\"", combinedContent, StringComparison.Ordinal);
        Assert.DoesNotContain("DbSet<Analytics", combinedContent, StringComparison.OrdinalIgnoreCase);
    }

    private static IReadOnlyCollection<string> ReadStringArray(JsonElement element, string propertyName) =>
        element
            .GetProperty(propertyName)
            .EnumerateArray()
            .Select(x => x.GetString() ?? string.Empty)
            .ToArray();
}
