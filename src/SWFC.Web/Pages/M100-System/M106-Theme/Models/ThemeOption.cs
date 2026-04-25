namespace SWFC.Web.Pages.M100_System.M106_Theme.Models;

public sealed record ThemeOption(
    string Name,
    string DisplayNameKey,
    string DescriptionKey,
    bool IsDarkMode);
