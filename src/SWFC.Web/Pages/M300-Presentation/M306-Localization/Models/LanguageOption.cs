namespace SWFC.Web.Pages.M300_Presentation.M306_Localization.Models;

public sealed record LanguageOption(
    string CultureName,
    string DisplayName,
    string NativeName,
    bool IsDefault);