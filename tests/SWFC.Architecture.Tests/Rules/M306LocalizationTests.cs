using System.Text.Json;
using SWFC.Architecture.Tests.Support;

namespace SWFC.Architecture.Tests.Rules;

public sealed class M306LocalizationTests
{
    [Fact]
    public void M306_Localization_Should_Expose_Core_Page_Services_And_Resources()
    {
        var requiredFiles = new[]
        {
            RepositoryRoot.Combine("src", "SWFC.Web", "Pages", "M300-Presentation", "M306-Localization", "Localization.razor"),
            RepositoryRoot.Combine("src", "SWFC.Web", "Pages", "M300-Presentation", "M306-Localization", "Localization.razor.css"),
            RepositoryRoot.Combine("src", "SWFC.Web", "Pages", "M300-Presentation", "M306-Localization", "Services", "LocalizationTextProvider.cs"),
            RepositoryRoot.Combine("src", "SWFC.Web", "Pages", "M300-Presentation", "M306-Localization", "Services", "CultureFormattingService.cs"),
            RepositoryRoot.Combine("src", "SWFC.Web", "Pages", "M300-Presentation", "M306-Localization", "Services", "LanguageCatalogService.cs"),
            RepositoryRoot.Combine("src", "SWFC.Web", "Pages", "M300-Presentation", "M306-Localization", "Services", "LocalizationValidationMessageCatalog.cs"),
            RepositoryRoot.Combine("src", "SWFC.Web", "Pages", "M300-Presentation", "M306-Localization", "Services", "NavigationModuleLabelService.cs")
        };

        var missingFiles = requiredFiles.Where(path => !File.Exists(path))
            .Select(RepositoryRoot.ToRelativePath)
            .ToArray();

        Assert.True(
            missingFiles.Length == 0,
            $"M306 documented artifacts are missing: {string.Join(", ", missingFiles)}");
    }

    [Fact]
    public void M306_Localization_Should_Keep_Required_Text_Resources_In_Every_Language()
    {
        var requiredKeys = new[]
        {
            "Common.Save",
            "Common.Cancel",
            "Common.Language",
            "Localization.Title",
            "Localization.CurrentLanguage",
            "Localization.AvailableLanguages",
            "Localization.FormatPreview",
            "Validation.Required",
            "Validation.InvalidValue"
        };

        var resourceFiles = Directory.EnumerateFiles(
            RepositoryRoot.Combine("src", "SWFC.Web", "Pages", "M300-Presentation", "M306-Localization", "Resources"),
            "*.json")
            .ToArray();

        Assert.True(resourceFiles.Length >= 2, "M306 needs at least two localization resource files.");

        var missingKeys = resourceFiles
            .SelectMany(path =>
            {
                var keys = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path))
                    ?? new Dictionary<string, string>();

                return requiredKeys
                    .Where(key => !keys.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
                    .Select(key => $"{RepositoryRoot.ToRelativePath(path)}:{key}");
            })
            .ToArray();

        Assert.True(
            missingKeys.Length == 0,
            $"M306 localization resources are missing required keys: {string.Join(", ", missingKeys)}");
    }

    [Fact]
    public void M306_Localization_Should_Link_Sidebar_To_The_Localization_Page()
    {
        var sidebarJson = File.ReadAllText(RepositoryRoot.Combine("src", "SWFC.Web", "wwwroot", "data", "sidebar.json"));

        Assert.Contains("\"code\": \"M306\"", sidebarJson, StringComparison.Ordinal);
        Assert.Contains("\"isActive\": true", sidebarJson, StringComparison.Ordinal);
        Assert.Contains("\"route\": \"/presentation/localization\"", sidebarJson, StringComparison.Ordinal);
    }
}
