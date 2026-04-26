namespace SWFC.Domain.M200_Business.M211_Analytics;

public enum AnalyticsDepartment
{
    Global,
    Maintenance,
    Energy,
    Quality,
    Inventory,
    Production
}

public sealed record KpiDefinition(
    string Code,
    string Title,
    string SourceModule,
    AnalyticsDepartment Department,
    string Unit,
    bool IsExportable)
{
    public bool UsesExternalFacts => !string.Equals(SourceModule, "M211", StringComparison.OrdinalIgnoreCase);
}

public sealed record ReportDefinition(
    string Code,
    string Title,
    string Visualization,
    IReadOnlyList<string> SourceModules,
    IReadOnlyList<string> ExportFormats)
{
    public bool HasNoOwnedFacts => SourceModules.Count > 0 && SourceModules.All(x => !string.Equals(x, "M211", StringComparison.OrdinalIgnoreCase));
}

public sealed record DashboardDefinition(
    AnalyticsDepartment Department,
    IReadOnlyList<KpiDefinition> Kpis,
    IReadOnlyList<ReportDefinition> Reports)
{
    public bool IsDepartmentSpecific => Department != AnalyticsDepartment.Global;
}

public sealed record AnalyticsSourceSnapshot(
    decimal EnergyConsumption,
    int OpenMaintenanceOrders,
    int OpenQualityCases,
    int InventoryBelowMinimum,
    int OpenProjectMeasures);

public sealed record KpiResult(string Code, string Title, decimal Value, string Unit, string SourceModule);

public static class AnalyticsCalculator
{
    public static IReadOnlyList<KpiResult> CalculateOperationalKpis(AnalyticsSourceSnapshot snapshot)
    {
        return
        [
            new KpiResult(
                "M205_ENERGY_TOTAL",
                "Energieverbrauch",
                Math.Round(snapshot.EnergyConsumption, 2, MidpointRounding.AwayFromZero),
                "kWh",
                "M205"),
            new KpiResult("M202_OPEN_ORDERS", "Offene Wartungsauftraege", snapshot.OpenMaintenanceOrders, "count", "M202"),
            new KpiResult("M207_OPEN_CASES", "Offene Qualitaetsfaelle", snapshot.OpenQualityCases, "count", "M207"),
            new KpiResult("M204_MIN_STOCK", "Bestand unter Mindestbestand", snapshot.InventoryBelowMinimum, "count", "M204"),
            new KpiResult("M209_OPEN_MEASURES", "Offene Massnahmen", snapshot.OpenProjectMeasures, "count", "M209")
        ];
    }
}
