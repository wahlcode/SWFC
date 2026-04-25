using SWFC.Web.Pages.M100_System.M106_Theme.Models;

namespace SWFC.Web.Pages.M100_System.M106_Theme.Services;

public sealed class ThemeCatalogService
{
    public const string DefaultThemeName = "dark";

    private static readonly ThemeOption[] ThemeOptions =
    [
        new("dark", "Theme.Option.Dark", "Theme.Option.Dark.Description", true),
        new("light", "Theme.Option.Light", "Theme.Option.Light.Description", false),
        new("high-contrast", "Theme.Option.HighContrast", "Theme.Option.HighContrast.Description", true)
    ];

    public IReadOnlyList<ThemeOption> GetThemeOptions() => ThemeOptions;

    public bool IsKnownTheme(string? themeName)
    {
        return ThemeOptions.Any(option =>
            string.Equals(option.Name, themeName?.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public string NormalizeThemeName(string? themeName)
    {
        if (string.IsNullOrWhiteSpace(themeName))
        {
            return DefaultThemeName;
        }

        var normalized = themeName.Trim();

        return IsKnownTheme(normalized)
            ? normalized
            : DefaultThemeName;
    }
}
