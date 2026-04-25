using SWFC.Web.Pages.M100_System.M104_Documents.Services;
using SWFC.Web.Pages.M100_System.M105_Configuration.Services;
using SWFC.Web.Pages.M300_Presentation.M302_Reporting.Services;
using SWFC.Web.Pages.M300_Presentation.M303_Notification.Services;
using SWFC.Web.Pages.M300_Presentation.M304_Search.Models;

namespace SWFC.Web.Pages.M300_Presentation.M304_Search.Services;

public sealed class SearchWorkspaceService
{
    private readonly DocumentWorkspaceService _documents;
    private readonly ConfigurationWorkspaceService _configuration;
    private readonly ReportingWorkspaceService _reports;
    private readonly NotificationWorkspaceService _notifications;

    public SearchWorkspaceService(
        DocumentWorkspaceService documents,
        ConfigurationWorkspaceService configuration,
        ReportingWorkspaceService reports,
        NotificationWorkspaceService notifications)
    {
        _documents = documents;
        _configuration = configuration;
        _reports = reports;
        _notifications = notifications;
    }

    public IReadOnlyList<SearchResultRecord> Search(SearchRequest request)
    {
        var query = request.Query?.Trim() ?? string.Empty;
        var moduleFilter = request.ModuleFilter?.Trim() ?? string.Empty;
        var allowedModules = request.AllowedModuleCodes
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code.Trim().ToUpperInvariant())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (allowedModules.Count == 0)
        {
            return Array.Empty<SearchResultRecord>();
        }

        return BuildIndex()
            .Where(result => allowedModules.Contains(result.ModuleCode))
            .Where(result => string.IsNullOrWhiteSpace(moduleFilter) ||
                             string.Equals(result.ModuleCode, moduleFilter, StringComparison.OrdinalIgnoreCase))
            .Where(result => Matches(result, query))
            .OrderByDescending(result => result.UpdatedUtc)
            .ThenBy(result => result.Title, StringComparer.OrdinalIgnoreCase)
            .Take(50)
            .ToArray();
    }

    private IEnumerable<SearchResultRecord> BuildIndex()
    {
        foreach (var document in _documents.GetDocuments())
        {
            yield return new SearchResultRecord(
                document.OwnerModule,
                "Document",
                document.Title,
                $"{document.Category}; versions {document.Versions.Count}; links {document.Links.Count}",
                "/system/documents",
                document.UpdatedUtc);
        }

        foreach (var setting in _configuration.GetSettings())
        {
            yield return new SearchResultRecord(
                setting.Scope,
                "Configuration",
                setting.Key,
                $"v{setting.Version}; security relevant: {setting.IsSecurityRelevant}",
                "/system/configuration",
                setting.UpdatedUtc);
        }

        foreach (var module in _configuration.GetModuleActivations())
        {
            yield return new SearchResultRecord(
                module.ModuleCode,
                "Module activation",
                module.ModuleCode,
                module.IsEnabled ? "Enabled" : "Disabled",
                "/system/configuration",
                module.UpdatedUtc);
        }

        foreach (var report in _reports.GetDefinitions())
        {
            yield return new SearchResultRecord(
                report.SourceModule,
                "Report",
                report.Name,
                $"{report.Visualization}; {report.FilterDefinition}",
                "/presentation/reports",
                report.CreatedUtc);
        }

        foreach (var notification in _notifications.GetNotifications())
        {
            yield return new SearchResultRecord(
                notification.RelatedModule,
                "Notification",
                notification.Title,
                $"{notification.Priority}; {notification.State}; {notification.TargetKind}: {notification.TargetValue}",
                "/presentation/notifications",
                notification.CreatedUtc);
        }
    }

    private static bool Matches(SearchResultRecord result, string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return true;
        }

        return result.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
               result.Summary.Contains(query, StringComparison.OrdinalIgnoreCase) ||
               result.ResultType.Contains(query, StringComparison.OrdinalIgnoreCase) ||
               result.ModuleCode.Contains(query, StringComparison.OrdinalIgnoreCase);
    }
}
