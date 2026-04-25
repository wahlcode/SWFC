namespace SWFC.Web.Pages.M300_Presentation.M302_Reporting.Models;

public sealed record ReportDefinition(
    Guid Id,
    string Name,
    string SourceModule,
    string Visualization,
    string FilterDefinition,
    IReadOnlyList<string> ExportFormats,
    DateTime CreatedUtc);

public sealed record ReportDefinitionRequest(
    string Name,
    string SourceModule,
    string Visualization,
    string FilterDefinition,
    IReadOnlyList<string> ExportFormats);
