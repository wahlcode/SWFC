using SWFC.Web.Pages.M100_System.M104_Documents.Models;
using SWFC.Web.Pages.M100_System.M104_Documents.Services;
using SWFC.Web.Pages.M100_System.M105_Configuration.Services;
using SWFC.Web.Pages.M300_Presentation.M302_Reporting.Models;
using SWFC.Web.Pages.M300_Presentation.M302_Reporting.Services;
using SWFC.Web.Pages.M300_Presentation.M303_Notification.Models;
using SWFC.Web.Pages.M300_Presentation.M303_Notification.Services;
using SWFC.Web.Pages.M300_Presentation.M304_Search.Models;
using SWFC.Web.Pages.M300_Presentation.M304_Search.Services;
using SWFC.Architecture.Tests.Support;
using System.Text.Json;

namespace SWFC.Architecture.Tests.Rules;

public sealed class M104_M105_M302_M303_M304_V040ImplementationTests
{
    [Fact]
    public void V040_Roadmap_Should_Be_Marked_Done_With_All_Milestones_Done()
    {
        using var roadmap = JsonDocument.Parse(File.ReadAllText(
            RepositoryRoot.Combine("docs", "M600-Planning", "M601-Roadmap", "roadmap.json")));

        var version = roadmap.RootElement
            .GetProperty("Versions")
            .EnumerateArray()
            .Single(x => x.GetProperty("Version").GetString() == "v0.4.0");

        Assert.Equal("Done", version.GetProperty("Status").GetString());
        Assert.All(
            new[] { "M104", "M105", "M302", "M303", "M304" },
            module => Assert.Contains(module, ReadStringArray(version, "PrimaryModules")));
        Assert.All(
            version.GetProperty("Milestones").EnumerateArray(),
            milestone => Assert.Equal("Done", milestone.GetProperty("Status").GetString()));
    }

    [Fact]
    public void V040_Modules_Should_Have_All_WorkItems_Done()
    {
        using var modules = JsonDocument.Parse(File.ReadAllText(
            RepositoryRoot.Combine("src", "SWFC.Web", "wwwroot", "data", "modules.json")));

        foreach (var moduleCode in new[] { "M104", "M105", "M302", "M303", "M304" })
        {
            var module = FindModule(modules, moduleCode);

            Assert.Equal("Full Complete", module.GetProperty("Status").GetString());
            Assert.Equal(100, module.GetProperty("ProgressPercent").GetInt32());
            Assert.All(
                module.GetProperty("WorkItems").EnumerateArray(),
                item => Assert.Equal("Done", item.GetProperty("Status").GetString()));
        }
    }

    [Fact]
    public void M104_Documents_Should_Register_Version_Link_And_History_Without_Delete_Path()
    {
        var service = new DocumentWorkspaceService();
        var document = service.RegisterDocument(new DocumentRegistrationRequest(
            "Work instruction",
            "Procedure",
            "M104",
            "Versioned retention"));

        var versioned = service.AddVersion(new DocumentVersionRequest(
            document.Id,
            "work-instruction.pdf",
            "application/pdf",
            2048,
            "sha256:abc",
            "Initial controlled version"));

        var linked = service.LinkDocument(new DocumentLinkRequest(
            document.Id,
            "M201",
            "Machine",
            "MX-001",
            "AppliesTo"));

        Assert.Equal(document.Id, linked.Id);
        Assert.Single(versioned.Versions);
        Assert.Single(linked.Links);
        Assert.Contains(linked.History, entry => entry.Action == "Registered");
        Assert.Contains(linked.History, entry => entry.Action == "VersionAdded");
        Assert.Contains(linked.History, entry => entry.Action == "Linked");
        Assert.DoesNotContain(
            typeof(DocumentWorkspaceService).GetMethods().Select(method => method.Name),
            methodName => methodName.Contains("Delete", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void M104_Documents_Should_Reject_Invalid_Version_Size()
    {
        var service = new DocumentWorkspaceService();
        var document = service.RegisterDocument(new DocumentRegistrationRequest(
            "Manual",
            "Instruction",
            "M104",
            "Versioned retention"));

        Assert.Throws<ArgumentOutOfRangeException>(() => service.AddVersion(new DocumentVersionRequest(
            document.Id,
            "manual.pdf",
            "application/pdf",
            0,
            "sha256:def",
            "Invalid version")));
    }

    [Fact]
    public void M105_Configuration_Should_Version_Settings_And_Module_Activation()
    {
        var service = new ConfigurationWorkspaceService();

        var setting = service.SetSetting(
            "Search.MaxResults",
            "25",
            "Limit search result payload.");
        var activation = service.SetModuleActivation(
            "M304",
            false,
            "Disable search surface for maintenance.");

        Assert.Equal("25", setting.Value);
        Assert.True(setting.Version > 1);
        Assert.Contains(setting.History, entry => entry.NewValue == "25");
        Assert.False(activation.IsEnabled);
        Assert.Equal("M304", activation.ModuleCode);
    }

    [Fact]
    public void M302_Reporting_Should_Register_Report_Definitions_With_Exports()
    {
        var service = new ReportingWorkspaceService();

        var report = service.Register(new ReportDefinitionRequest(
            "Inventory overview",
            "M204",
            "Table",
            "Status and location filters",
            ["PDF", "Excel"]));

        Assert.Equal("M204", report.SourceModule);
        Assert.Contains("Excel", report.ExportFormats);
        Assert.Contains(service.GetDefinitions(), item => item.Id == report.Id);
    }

    [Fact]
    public void M303_Notifications_Should_Publish_Change_State_And_Reject_Missing_Title()
    {
        var service = new NotificationWorkspaceService();

        var notification = service.Publish(new NotificationRequest(
            "Critical configuration change",
            "A security relevant setting changed.",
            "Warning",
            "High",
            "Role",
            "Administrators",
            "UI",
            "M105",
            "Search.MaxResults"));

        var read = service.MarkRead(notification.Id);
        var done = service.Complete(notification.Id);

        Assert.Equal(NotificationState.Read, read.State);
        Assert.Equal(NotificationState.Done, done.State);
        Assert.Throws<ArgumentException>(() => service.Publish(new NotificationRequest(
            string.Empty,
            "Missing title",
            "Info",
            "Low",
            "User",
            "admin",
            "UI",
            "M303",
            "system")));
    }

    [Fact]
    public void M304_Search_Should_Filter_By_Permission_Module()
    {
        var documents = new DocumentWorkspaceService();
        var configuration = new ConfigurationWorkspaceService();
        var reports = new ReportingWorkspaceService();
        var notifications = new NotificationWorkspaceService();
        var search = new SearchWorkspaceService(documents, configuration, reports, notifications);

        documents.RegisterDocument(new DocumentRegistrationRequest(
            "Calibration certificate",
            "Certificate",
            "M104",
            "Versioned retention"));
        notifications.Publish(new NotificationRequest(
            "Access review",
            "Review pending.",
            "Info",
            "Medium",
            "Role",
            "Security",
            "UI",
            "M806",
            "review"));

        var results = search.Search(new SearchRequest(
            "review",
            string.Empty,
            ["M104"]));

        Assert.DoesNotContain(results, result => result.ModuleCode == "M806");
    }

    [Fact]
    public void V040_UI_Should_Be_Routed_And_Callable_From_Sidebar_And_Program()
    {
        var requiredFiles = new[]
        {
            RepositoryRoot.Combine("src", "SWFC.Web", "Pages", "M100-System", "M104-Documents", "Documents.razor"),
            RepositoryRoot.Combine("src", "SWFC.Web", "Pages", "M100-System", "M105-Configuration", "Configuration.razor"),
            RepositoryRoot.Combine("src", "SWFC.Web", "Pages", "M300-Presentation", "M302-Reporting", "Reporting.razor"),
            RepositoryRoot.Combine("src", "SWFC.Web", "Pages", "M300-Presentation", "M303-Notification", "Notifications.razor"),
            RepositoryRoot.Combine("src", "SWFC.Web", "Pages", "M300-Presentation", "M304-Search", "Search.razor")
        };

        Assert.All(requiredFiles, file => Assert.True(File.Exists(file), file));

        var sidebar = File.ReadAllText(RepositoryRoot.Combine("src", "SWFC.Web", "wwwroot", "data", "sidebar.json"));
        var program = File.ReadAllText(RepositoryRoot.Combine("src", "SWFC.Web", "Program.cs"));

        Assert.Contains("\"route\": \"/system/documents\"", sidebar, StringComparison.Ordinal);
        Assert.Contains("\"route\": \"/system/configuration\"", sidebar, StringComparison.Ordinal);
        Assert.Contains("\"route\": \"/presentation/reports\"", sidebar, StringComparison.Ordinal);
        Assert.Contains("\"route\": \"/presentation/notifications\"", sidebar, StringComparison.Ordinal);
        Assert.Contains("\"route\": \"/presentation/search\"", sidebar, StringComparison.Ordinal);
        Assert.Contains("/system/documents/register", program, StringComparison.Ordinal);
        Assert.Contains("/system/configuration/setting", program, StringComparison.Ordinal);
        Assert.Contains("/presentation/reports/register", program, StringComparison.Ordinal);
        Assert.Contains("/presentation/notifications/publish", program, StringComparison.Ordinal);
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
