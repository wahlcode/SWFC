using SWFC.Web.Pages.M100_System.M106_Theme.Services;

namespace SWFC.Web.Pages.M300_Presentation.M307_User_Personalization.Models;

public sealed record UserUiPreferences(
    string ThemeName,
    string StartPageRoute,
    string[] DashboardWidgetKeys,
    string NavigationDensity,
    bool UseReducedMotion)
{
    public const string ComfortableDensity = "comfortable";
    public const string CompactDensity = "compact";

    public static readonly string[] AllowedStartPageRoutes =
    [
        "/dashboard",
        "/planning/modules",
        "/roadmap",
        "/presentation/personalization"
    ];

    public static readonly string[] AllowedDashboardWidgetKeys =
    [
        "organization",
        "assets",
        "maintenance",
        "inventory",
        "energy",
        "modules",
        "roadmap",
        "quicklinks",
        "status"
    ];

    public static UserUiPreferences Default { get; } = new(
        ThemeCatalogService.DefaultThemeName,
        "/dashboard",
        AllowedDashboardWidgetKeys,
        ComfortableDensity,
        false);

    public static UserUiPreferences Normalize(
        UserUiPreferences? preferences,
        ThemeCatalogService themeCatalogService)
    {
        if (preferences is null)
        {
            return Default;
        }

        var themeName = themeCatalogService.NormalizeThemeName(preferences.ThemeName);
        var startPageRoute = AllowedStartPageRoutes.Contains(preferences.StartPageRoute, StringComparer.OrdinalIgnoreCase)
            ? preferences.StartPageRoute
            : Default.StartPageRoute;
        var density = string.Equals(preferences.NavigationDensity, CompactDensity, StringComparison.OrdinalIgnoreCase)
            ? CompactDensity
            : ComfortableDensity;
        var widgetKeys = preferences.DashboardWidgetKeys
            .Where(key => AllowedDashboardWidgetKeys.Contains(key, StringComparer.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (widgetKeys.Length == 0)
        {
            widgetKeys = Default.DashboardWidgetKeys;
        }

        return new UserUiPreferences(
            themeName,
            startPageRoute,
            widgetKeys,
            density,
            preferences.UseReducedMotion);
    }

    public bool HasDashboardWidget(string key)
    {
        return DashboardWidgetKeys.Contains(key, StringComparer.OrdinalIgnoreCase);
    }
}

public sealed record UserUiPreferenceEnvelope(
    string UserId,
    UserUiPreferences Preferences);
