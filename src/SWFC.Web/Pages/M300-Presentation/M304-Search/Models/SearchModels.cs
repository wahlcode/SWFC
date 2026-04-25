namespace SWFC.Web.Pages.M300_Presentation.M304_Search.Models;

public sealed record SearchRequest(
    string Query,
    string ModuleFilter,
    IReadOnlyCollection<string> AllowedModuleCodes);

public sealed record SearchResultRecord(
    string ModuleCode,
    string ResultType,
    string Title,
    string Summary,
    string Route,
    DateTime UpdatedUtc);
