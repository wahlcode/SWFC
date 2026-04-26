using System.Text.Json;
using SWFC.Application.M1000_Platform.M1001_PluginSystem;
using SWFC.Application.M1000_Platform.M1002_DeveloperPlatformSdk;
using SWFC.Application.M1000_Platform.M1003_ExtensionManagement;
using SWFC.Application.M1000_Platform.M1004_Marketplace;
using SWFC.Application.M1000_Platform.M1005_VersioningUpdateManagement;
using SWFC.Architecture.Tests.Support;

namespace SWFC.Architecture.Tests.Rules;

public sealed class M1001_M1005_V019PlatformTests
{
    [Fact]
    public void M1001_Should_Load_Plugin_Through_Registered_Extension_Point_And_Isolate_Failures()
    {
        var catalog = new PluginCatalog();
        catalog.RegisterExtensionPoint("platform.navigation");
        var plugin = new TestPlugin(new PluginManifest(
            "extension.navigation.cards",
            "Navigation Cards",
            new PluginVersion(1, 0, 0),
            [new PluginCapability("navigation.cards", "platform.navigation", RequiresRuntimeAccess: false)],
            new PluginVersion(0, 19, 0),
            new PluginVersion(0, 21, 0)));

        var result = catalog.Load(plugin, new PluginVersion(0, 19, 0), Now());
        var blocked = catalog.Load(new TestPlugin(plugin.Manifest with
        {
            PluginId = "extension.direct.runtime",
            Capabilities = [new PluginCapability("runtime", "platform.navigation", RequiresRuntimeAccess: true)]
        }), new PluginVersion(0, 19, 0), Now());

        Assert.Equal(PluginLifecycleState.Loaded, result.State);
        Assert.True(result.IsolatedFromCore);
        Assert.True(plugin.Initialized);
        Assert.Equal(PluginLifecycleState.Failed, blocked.State);
        Assert.Contains("direct runtime access", blocked.Evidence, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(catalog.GetLifecycleHistory(), item => item.State == PluginLifecycleState.Failed);
    }

    [Fact]
    public void M1002_Should_Validate_Sdk_Contracts_And_Create_Project_Descriptor()
    {
        var sdk = new DeveloperPlatformSdk();
        var profile = new ExtensionDevelopmentProfile(
            "developer.internal",
            "extension.audit.cards",
            new PluginVersion(0, 19, 0),
            ["M1001.PluginContracts", "M1003.ExtensionLifecycle"],
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["UsesOnlyPlatformContracts"] = "true"
            });

        var report = sdk.Validate(profile);
        var descriptor = sdk.CreateProjectDescriptor(profile);

        Assert.True(report.IsCompliant);
        Assert.Empty(report.Violations);
        Assert.Contains("plugin.manifest.json", descriptor.RequiredFiles);
        Assert.Equal(report.ApprovedApiCodes, descriptor.ApiContracts);
    }

    [Fact]
    public void M1003_Should_Control_Extension_Lifecycle_With_Audit_History()
    {
        var manager = new ExtensionLifecycleManager();
        var installed = manager.Install(new ExtensionRegistration(
            "extension.marketplace.sync",
            "Marketplace Sync",
            new PluginVersion(1, 2, 0),
            "marketplace://extensions/extension.marketplace.sync",
            Now()));

        var active = manager.Activate(installed.ExtensionId, Now().AddMinutes(1), "Release approved.");
        var inactive = manager.Deactivate(installed.ExtensionId, Now().AddMinutes(2), "Operator disabled extension.");

        Assert.Equal(ExtensionLifecycleStatus.Installed, installed.Status);
        Assert.Equal(ExtensionLifecycleStatus.Active, active.Status);
        Assert.Equal(ExtensionLifecycleStatus.Inactive, inactive.Status);
        Assert.Equal(3, manager.GetAuditTrail().Count);
    }

    [Fact]
    public void M1004_Should_Catalog_Available_Extensions_Without_Auto_Activation()
    {
        var marketplace = new MarketplaceCatalog();
        marketplace.Publish(new MarketplaceExtensionPackage(
            "pkg-maintenance-dashboard",
            "extension.maintenance.dashboard",
            "Maintenance Dashboard",
            new PluginVersion(1, 1, 0),
            ["Operations", "Maintenance"],
            new PluginVersion(0, 19, 0),
            new PluginVersion(0, 21, 0),
            IsApprovedForDistribution: true));

        var results = marketplace.Search(new MarketplaceSearchRequest(
            "Operations",
            new PluginVersion(0, 20, 0),
            ApprovedOnly: true));

        var result = Assert.Single(results);
        Assert.True(result.IsCompatible);
        Assert.StartsWith("marketplace://", result.DistributionReference, StringComparison.Ordinal);
    }

    [Fact]
    public void M1005_Should_Evaluate_And_Apply_Platform_Update_With_Version_History()
    {
        var versioning = new PlatformVersioningService();
        versioning.RegisterCurrent(new PlatformVersionState(
            new PluginVersion(0, 19, 0),
            "stable",
            Now(),
            ["v0.19.0-platform"]));

        var decision = versioning.Evaluate(new PlatformUpdatePackage(
            "platform-update-020",
            new PluginVersion(0, 19, 0),
            new PluginVersion(0, 20, 0),
            "stable",
            ["M804", "M805"],
            RolloutControlled: true));
        var applied = versioning.Apply("platform-update-020", Now().AddMinutes(5));

        Assert.Equal(PlatformUpdateStatus.Compatible, decision.Status);
        Assert.Equal(PlatformUpdateStatus.Applied, applied.Status);
        Assert.Equal(new PluginVersion(0, 20, 0), versioning.GetVersionHistory().Last().Version);
    }

    [Fact]
    public void V019_Roadmap_And_Platform_Modules_Should_Be_Marked_Done_After_Verification()
    {
        using var roadmap = JsonDocument.Parse(File.ReadAllText(
            RepositoryRoot.Combine("docs", "M600-Planning", "M601-Roadmap", "roadmap.json")));
        using var modules = JsonDocument.Parse(File.ReadAllText(
            RepositoryRoot.Combine("src", "SWFC.Web", "wwwroot", "data", "modules.json")));

        var v019 = roadmap.RootElement
            .GetProperty("Versions")
            .EnumerateArray()
            .Single(version => version.GetProperty("Version").GetString() == "v0.19.0");

        Assert.Equal("Done", v019.GetProperty("Status").GetString());
        Assert.All(v019.GetProperty("Milestones").EnumerateArray(), milestone =>
            Assert.Equal("Done", milestone.GetProperty("Status").GetString()));

        foreach (var moduleCode in new[] { "M1001", "M1002", "M1003", "M1004", "M1005" })
        {
            var module = modules.RootElement
                .GetProperty("Groups")
                .EnumerateArray()
                .SelectMany(group => group.GetProperty("Modules").EnumerateArray())
                .Single(item => item.GetProperty("Code").GetString() == moduleCode);

            Assert.Equal("Full Complete", module.GetProperty("Status").GetString());
            Assert.Equal(100, module.GetProperty("ProgressPercent").GetInt32());
            Assert.All(module.GetProperty("WorkItems").EnumerateArray(), workItem =>
                Assert.Equal("Done", workItem.GetProperty("Status").GetString()));
        }
    }

    [Fact]
    public void M1000_Platform_Should_Not_Contain_Business_Db_Or_Placeholder_Coupling()
    {
        var files = RepositoryRoot.EnumerateFiles(
                Path.Combine("src", "SWFC.Application", "M1000-Platform"),
                "*.cs",
                includeGeneratedFiles: false)
            .ToArray();

        Assert.NotEmpty(files);

        var combined = string.Join(Environment.NewLine, files.Select(File.ReadAllText));
        Assert.DoesNotContain("TODO", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Placeholder", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("DbContext", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("UseNpgsql", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("SWFC.Application.M200", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("SWFC.Domain.M200", combined, StringComparison.Ordinal);
    }

    private static DateTimeOffset Now() => new(2026, 4, 26, 12, 0, 0, TimeSpan.Zero);

    private sealed class TestPlugin(PluginManifest manifest) : IPlatformPlugin
    {
        public PluginManifest Manifest { get; } = manifest;

        public bool Initialized { get; private set; }

        public void Initialize(PluginRuntimeContext context)
        {
            Assert.Equal(Manifest.PluginId, context.PluginId);
            Assert.Equal("CoreContractsOnly", context.Settings["Isolation"]);
            Initialized = true;
        }

        public void Shutdown()
        {
            Initialized = false;
        }
    }
}
