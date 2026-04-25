using SWFC.Web.Pages.M300_Presentation.M302_Reporting.Models;

namespace SWFC.Web.Pages.M300_Presentation.M302_Reporting.Services;

public sealed class ReportingWorkspaceService
{
    private readonly object _gate = new();
    private readonly Dictionary<Guid, ReportDefinition> _definitions = new();

    public ReportingWorkspaceService()
    {
        Register(new ReportDefinitionRequest(
            "Roadmap verification",
            "M601",
            "Timeline",
            "Version, status and milestone filter",
            ["PDF", "Excel"]));
        Register(new ReportDefinitionRequest(
            "Audit activity",
            "M805",
            "Table",
            "Actor, module and result filter",
            ["PDF", "Excel"]));
    }

    public ReportDefinition Register(ReportDefinitionRequest request)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Name);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.SourceModule);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Visualization);

        var definition = new ReportDefinition(
            Guid.NewGuid(),
            request.Name.Trim(),
            request.SourceModule.Trim().ToUpperInvariant(),
            request.Visualization.Trim(),
            string.IsNullOrWhiteSpace(request.FilterDefinition)
                ? "No filter"
                : request.FilterDefinition.Trim(),
            NormalizeExportFormats(request.ExportFormats),
            DateTime.UtcNow);

        lock (_gate)
        {
            _definitions[definition.Id] = definition;
        }

        return definition;
    }

    public IReadOnlyList<ReportDefinition> GetDefinitions()
    {
        lock (_gate)
        {
            return _definitions.Values
                .OrderBy(definition => definition.Name, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
    }

    private static IReadOnlyList<string> NormalizeExportFormats(IReadOnlyList<string> formats)
    {
        var normalized = formats
            .Where(format => !string.IsNullOrWhiteSpace(format))
            .Select(format => format.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return normalized.Length == 0 ? ["PDF"] : normalized;
    }
}
