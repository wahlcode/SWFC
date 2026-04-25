using System.Text.Json;
using SWFC.Architecture.Tests.Support;
using SWFC.Web.Pages.M100_System.M106_Theme.Services;
using SWFC.Web.Pages.M300_Presentation.M307_User_Personalization.Models;

namespace SWFC.Architecture.Tests.Rules;

public sealed class M106_M301_M305_M307_V030CompletionTests
{
    [Fact]
    public void V030_Roadmap_Should_Be_Marked_Done_With_All_Milestones_Done()
    {
        using var roadmap = JsonDocument.Parse(File.ReadAllText(
            RepositoryRoot.Combine("docs", "M600-Planning", "M601-Roadmap", "roadmap.json")));

        var version = roadmap.RootElement
            .GetProperty("Versions")
            .EnumerateArray()
            .Single(x => x.GetProperty("Version").GetString() == "v0.3.0");

        Assert.Equal("Done", version.GetProperty("Status").GetString());
        Assert.All(
            new[] { "M301", "M305", "M306", "M307", "M106" },
            module => Assert.Contains(module, ReadStringArray(version, "PrimaryModules")));
        Assert.All(
            version.GetProperty("Milestones").EnumerateArray(),
            milestone => Assert.Equal("Done", milestone.GetProperty("Status").GetString()));
    }

    [Fact]
    public void V030_Modules_Should_Have_All_WorkItems_Done()
    {
        using var modules = JsonDocument.Parse(File.ReadAllText(
            RepositoryRoot.Combine("src", "SWFC.Web", "wwwroot", "data", "modules.json")));

        foreach (var moduleCode in new[] { "M106", "M301", "M305", "M306", "M307" })
        {
            var module = FindModule(modules, moduleCode);

            Assert.Equal("Full Complete", module.GetProperty("Status").GetString());
            Assert.All(
                module.GetProperty("WorkItems").EnumerateArray(),
                item => Assert.Equal("Done", item.GetProperty("Status").GetString()));
        }
    }

    [Fact]
    public void M301_AppShell_Should_Expose_Global_Navigation_And_Role_Aware_Visibility()
    {
        var requiredFiles = new[]
        {
            RepositoryRoot.Combine("src", "SWFC.Web", "Components", "Layout", "AppShell.razor"),
            RepositoryRoot.Combine("src", "SWFC.Web", "Components", "Layout", "Header.razor"),
            RepositoryRoot.Combine("src", "SWFC.Web", "Components", "Layout", "Sidebar.razor"),
            RepositoryRoot.Combine("src", "SWFC.Web", "Pages", "M300-Presentation", "M301-AppShell", "Dashboard.razor")
        };

        Assert.All(requiredFiles, file => Assert.True(File.Exists(file), file));

        var sidebarJson = File.ReadAllText(
            RepositoryRoot.Combine("src", "SWFC.Web", "wwwroot", "data", "sidebar.json"));
        var header = File.ReadAllText(requiredFiles[1]);
        var sidebar = File.ReadAllText(requiredFiles[2]);

        Assert.Contains("\"code\": \"M301\"", sidebarJson, StringComparison.Ordinal);
        Assert.Contains("\"route\": \"/dashboard\"", sidebarJson, StringComparison.Ordinal);
        Assert.Contains("\"route\": \"/presentation/forms\"", sidebarJson, StringComparison.Ordinal);
        Assert.Contains("\"route\": \"/presentation/personalization\"", sidebarJson, StringComparison.Ordinal);
        Assert.Contains("HasModuleAccess", header, StringComparison.Ordinal);
        Assert.Contains("AccessModuleCodes", sidebar, StringComparison.Ordinal);
        Assert.Contains("SidebarAccessModes", sidebar, StringComparison.Ordinal);
    }

    [Fact]
    public void M106_Theme_Should_Provide_Tokens_DarkMode_And_UserTheme_Application()
    {
        var themeCss = File.ReadAllText(
            RepositoryRoot.Combine("src", "SWFC.Web", "wwwroot", "css", "theme.css"));
        var appShell = File.ReadAllText(
            RepositoryRoot.Combine("src", "SWFC.Web", "Components", "Layout", "AppShell.razor"));

        Assert.Contains("[data-swfc-theme=\"light\"]", themeCss, StringComparison.Ordinal);
        Assert.Contains("[data-swfc-theme=\"high-contrast\"]", themeCss, StringComparison.Ordinal);
        Assert.Contains("[data-swfc-density=\"compact\"]", themeCss, StringComparison.Ordinal);
        Assert.Contains("[data-swfc-motion=\"reduced\"]", themeCss, StringComparison.Ordinal);
        Assert.Contains("data-swfc-theme", appShell, StringComparison.Ordinal);
        Assert.Contains("ThemeCatalogService", File.ReadAllText(
            RepositoryRoot.Combine("src", "SWFC.Web", "Pages", "M100-System", "M106-Theme", "Services", "ThemeCatalogService.cs")));
    }

    [Fact]
    public void M305_FormBasis_Should_Expose_Shared_Form_Mode_Field_And_Validation_Patterns()
    {
        var formShell = File.ReadAllText(
            RepositoryRoot.Combine("src", "SWFC.Web", "Pages", "M300-Presentation", "M305-Forms", "Components", "SwfcFormShell.razor"));
        var fieldBlock = File.ReadAllText(
            RepositoryRoot.Combine("src", "SWFC.Web", "Pages", "M300-Presentation", "M305-Forms", "Components", "SwfcFieldBlock.razor"));
        var personalization = File.ReadAllText(
            RepositoryRoot.Combine("src", "SWFC.Web", "Pages", "M300-Presentation", "M307-User-Personalization", "UserPersonalization.razor"));

        Assert.Contains("data-form-mode", formShell, StringComparison.Ordinal);
        Assert.Contains("ValidationErrors", formShell, StringComparison.Ordinal);
        Assert.Contains("Required", fieldBlock, StringComparison.Ordinal);
        Assert.Contains("role=\"alert\"", fieldBlock, StringComparison.Ordinal);
        Assert.Contains("SwfcFormShell", personalization, StringComparison.Ordinal);
        Assert.Contains("SwfcFieldBlock", personalization, StringComparison.Ordinal);
    }

    [Fact]
    public void M307_Personalization_Should_Persist_User_Preferences_And_Control_Dashboard_Widgets()
    {
        var service = File.ReadAllText(
            RepositoryRoot.Combine("src", "SWFC.Web", "Pages", "M300-Presentation", "M307-User-Personalization", "Services", "UserUiPreferenceService.cs"));
        var program = File.ReadAllText(
            RepositoryRoot.Combine("src", "SWFC.Web", "Program.cs"));
        var dashboard = File.ReadAllText(
            RepositoryRoot.Combine("src", "SWFC.Web", "Pages", "M300-Presentation", "M301-AppShell", "Dashboard.razor"));

        Assert.Contains("IDataProtector", service, StringComparison.Ordinal);
        Assert.Contains(".SWFC.M307.UserUiPreferences", service, StringComparison.Ordinal);
        Assert.Contains("Response.Cookies.Append", service, StringComparison.Ordinal);
        Assert.Contains("/presentation/preferences/save", program, StringComparison.Ordinal);
        Assert.Contains("ValidateRequestAsync", program, StringComparison.Ordinal);
        Assert.Contains("HasDashboardWidget", dashboard, StringComparison.Ordinal);
        Assert.Contains("StartPageRoute", dashboard, StringComparison.Ordinal);
        Assert.Contains("NavigateTo", dashboard, StringComparison.Ordinal);
    }

    [Fact]
    public void M307_UserUiPreferences_Should_Normalize_Invalid_Input()
    {
        var normalized = UserUiPreferences.Normalize(
            new UserUiPreferences(
                "light",
                "/roadmap",
                ["assets", "unknown", "assets"],
                UserUiPreferences.CompactDensity,
                true),
            new ThemeCatalogService());

        Assert.Equal("light", normalized.ThemeName);
        Assert.Equal("/roadmap", normalized.StartPageRoute);
        Assert.Equal(UserUiPreferences.CompactDensity, normalized.NavigationDensity);
        Assert.True(normalized.UseReducedMotion);
        Assert.Equal(new[] { "assets" }, normalized.DashboardWidgetKeys);
    }

    private static JsonElement FindModule(JsonDocument modules, string moduleCode)
    {
        return modules.RootElement
            .GetProperty("Groups")
            .EnumerateArray()
            .SelectMany(group => group.GetProperty("Modules").EnumerateArray())
            .Single(module => module.GetProperty("Code").GetString() == moduleCode);
    }

    private static IReadOnlyCollection<string> ReadStringArray(JsonElement element, string propertyName) =>
        element
            .GetProperty(propertyName)
            .EnumerateArray()
            .Select(x => x.GetString() ?? string.Empty)
            .ToArray();
}
