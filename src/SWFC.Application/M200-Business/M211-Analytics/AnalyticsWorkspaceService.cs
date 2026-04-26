using SWFC.Domain.M200_Business.M211_Analytics;

namespace SWFC.Application.M200_Business.M211_Analytics;

public sealed record AnalyticsOverviewDto(
    IReadOnlyList<KpiResult> Kpis,
    IReadOnlyList<DashboardDefinition> Dashboards,
    IReadOnlyList<ReportDefinition> Reports);

public sealed class AnalyticsWorkspaceService
{
    private readonly IReadOnlyList<KpiDefinition> _kpiDefinitions =
    [
        new KpiDefinition("M205_ENERGY_TOTAL", "Energieverbrauch", "M205", AnalyticsDepartment.Energy, "kWh", true),
        new KpiDefinition("M202_OPEN_ORDERS", "Offene Wartungsauftraege", "M202", AnalyticsDepartment.Maintenance, "count", true),
        new KpiDefinition("M207_OPEN_CASES", "Offene Qualitaetsfaelle", "M207", AnalyticsDepartment.Quality, "count", true),
        new KpiDefinition("M204_MIN_STOCK", "Bestand unter Mindestbestand", "M204", AnalyticsDepartment.Inventory, "count", true),
        new KpiDefinition("M209_OPEN_MEASURES", "Offene Massnahmen", "M209", AnalyticsDepartment.Global, "count", true)
    ];

    private readonly IReadOnlyList<ReportDefinition> _reports =
    [
        new ReportDefinition("OPS_OVERVIEW", "Operative Uebersicht", "Table", ["M202", "M204", "M205", "M207", "M209"], ["PDF", "Excel"]),
        new ReportDefinition("ENERGY_TREND", "Energieauswertung", "LineChart", ["M205"], ["PDF", "Excel"]),
        new ReportDefinition("QUALITY_ACTIONS", "Qualitaetsmassnahmen", "BarChart", ["M207", "M209"], ["PDF"])
    ];

    public AnalyticsOverviewDto GetOverview(AnalyticsSourceSnapshot? snapshot = null)
    {
        var kpis = AnalyticsCalculator.CalculateOperationalKpis(snapshot ?? EmptySnapshot);
        var dashboards = Enum.GetValues<AnalyticsDepartment>()
            .Select(department => new DashboardDefinition(
                department,
                _kpiDefinitions.Where(x => x.Department == department || department == AnalyticsDepartment.Global).ToList(),
                _reports.Where(x => department == AnalyticsDepartment.Global || x.SourceModules.Contains(GetDepartmentSourceModule(department))).ToList()))
            .ToList();

        return new AnalyticsOverviewDto(kpis, dashboards, _reports);
    }

    private static AnalyticsSourceSnapshot EmptySnapshot => new(
        EnergyConsumption: 0,
        OpenMaintenanceOrders: 0,
        OpenQualityCases: 0,
        InventoryBelowMinimum: 0,
        OpenProjectMeasures: 0);

    private static string GetDepartmentSourceModule(AnalyticsDepartment department)
    {
        return department switch
        {
            AnalyticsDepartment.Maintenance => "M202",
            AnalyticsDepartment.Energy => "M205",
            AnalyticsDepartment.Quality => "M207",
            AnalyticsDepartment.Inventory => "M204",
            AnalyticsDepartment.Production => "M212",
            _ => "M209"
        };
    }
}
