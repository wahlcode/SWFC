using System.Globalization;
using SWFC.Web.Pages.M300_Presentation.M306_Localization.Models;

namespace SWFC.Web.Pages.M300_Presentation.M306_Localization.Services;

public sealed class LanguageCatalogService
{
    private static readonly HashSet<string> SupportedCultureNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "en-US",
        "de-DE",
        "es-MX",
        "hu-HU",
        "it-IT",
        "pl-PL",
        "ru-RU",
        "sr-RS"
    };

    private readonly IWebHostEnvironment _environment;

    public LanguageCatalogService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public IReadOnlyList<LanguageOption> GetAvailableLanguages()
    {
        var resourceDirectory = Path.Combine(
            _environment.ContentRootPath,
            "Pages",
            "M300-Presentation",
            "M306-Localization",
            "Resources");

        if (!Directory.Exists(resourceDirectory))
        {
            return Array.Empty<LanguageOption>();
        }

        return Directory.EnumerateFiles(resourceDirectory, "*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!)
            .Where(IsSupportedCulture)
            .OrderByDescending(x => string.Equals(x, LocalizationTextProvider.DefaultCultureName, StringComparison.OrdinalIgnoreCase))
            .ThenBy(x => x, StringComparer.OrdinalIgnoreCase)
            .Select(CreateLanguageOption)
            .ToList();
    }

    public bool IsSupportedCulture(string? cultureName)
    {
        return !string.IsNullOrWhiteSpace(cultureName) &&
            SupportedCultureNames.Contains(cultureName.Trim());
    }

    public string NormalizeCultureName(string? cultureName)
    {
        return IsSupportedCulture(cultureName)
            ? cultureName!.Trim()
            : LocalizationTextProvider.DefaultCultureName;
    }

    private static LanguageOption CreateLanguageOption(string cultureName)
    {
        var culture = CultureInfo.GetCultureInfo(cultureName);

        return new LanguageOption(
            culture.Name,
            culture.DisplayName,
            culture.NativeName,
            string.Equals(culture.Name, LocalizationTextProvider.DefaultCultureName, StringComparison.OrdinalIgnoreCase));
    }
}