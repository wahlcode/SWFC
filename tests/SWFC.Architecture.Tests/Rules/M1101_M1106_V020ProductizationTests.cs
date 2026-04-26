using System.Text.Json;
using SWFC.Application.M1000_Platform.M1001_PluginSystem;
using SWFC.Application.M1000_Platform.M1005_VersioningUpdateManagement;
using SWFC.Application.M1100_ProductizationDistribution.M1101_Distribution;
using SWFC.Application.M1100_ProductizationDistribution.M1102_Updates;
using SWFC.Application.M1100_ProductizationDistribution.M1103_ProductVersioning;
using SWFC.Application.M1100_ProductizationDistribution.M1104_Licensing;
using SWFC.Application.M1100_ProductizationDistribution.M1105_BackupRestore;
using SWFC.Application.M1100_ProductizationDistribution.M1106_ProductOperations;
using SWFC.Architecture.Tests.Support;

namespace SWFC.Architecture.Tests.Rules;

public sealed class M1101_M1106_V020ProductizationTests
{
    [Fact]
    public void M1101_Should_Prepare_Distribution_Package_With_Release_Evidence()
    {
        var service = new ProductDistributionService();
        var package = service.Prepare(new ProductDistributionRequest(
            "SWFC",
            new PluginVersion(0, 20, 0),
            new DistributionProfile("customer-stable", DistributionChannel.Customer, "customer-a", ["REL-020"], RequiresLicenseClearance: true),
            [new DistributionArtifact("web", "swfc-web.zip", "ABCDEF123456", 1024)],
            LicenseCleared: true));

        Assert.Equal("SWFC-0.20.0-customer-stable", package.PackageId);
        Assert.Contains("without setup", package.Evidence, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void M1102_Should_Release_And_Distribute_Product_Update_Against_Platform_State()
    {
        var orchestrator = new ProductUpdateOrchestrator();
        var decision = orchestrator.Release(
            new ProductUpdatePackage(
                "product-update-020",
                new PluginVersion(0, 19, 0),
                new PluginVersion(0, 20, 0),
                new PluginVersion(0, 19, 0),
                ["customer-a", "customer-b"],
                ProductReleaseApproved: true),
            new PlatformVersionState(new PluginVersion(0, 19, 0), "stable", Now(), ["v0.19.0"]));

        var distributed = orchestrator.MarkDistributed("product-update-020");

        Assert.Equal(ProductUpdateRolloutStatus.Released, decision.Status);
        Assert.Equal(ProductUpdateRolloutStatus.Distributed, distributed.Status);
        Assert.Equal(2, orchestrator.GetHistory().Count);
    }

    [Fact]
    public void M1103_Should_Record_Product_Version_In_Customer_Context()
    {
        var registry = new ProductVersionRegistry();
        registry.Record(new ProductVersionRecord(
            "SWFC",
            new PluginVersion(0, 20, 0),
            new PluginVersion(0, 19, 0),
            ProductVersionReleaseState.Released,
            "customer-a",
            Now()));
        var installed = registry.Record(new ProductVersionRecord(
            "SWFC",
            new PluginVersion(0, 20, 0),
            new PluginVersion(0, 19, 0),
            ProductVersionReleaseState.Installed,
            "customer-a",
            Now().AddMinutes(10)));

        Assert.Equal(installed, registry.GetInstalled("SWFC", "customer-a"));
        Assert.Equal(2, registry.GetHistory("SWFC").Count);
    }

    [Fact]
    public void M1104_Should_Evaluate_Product_License_Without_Security_Decision()
    {
        var service = new ProductLicensingService();
        var evaluation = service.Evaluate(new ProductLicense(
            "LIC-001",
            "SWFC",
            "customer-a",
            "Subscription",
            Now().AddDays(-1),
            Now().AddDays(30),
            ["Distribution", "Updates"],
            ProductLicenseStatus.Active), Now());

        Assert.True(evaluation.AllowsProductUse);
        Assert.Equal(ProductLicenseStatus.Active, evaluation.Status);
        Assert.Contains("authentication and authorization remain separate", evaluation.Evidence, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void M1105_Should_Create_Verified_Backup_And_Prepare_Approved_Restore()
    {
        var service = new ProductBackupRestoreService();
        var snapshot = service.CreateSnapshot(new BackupScope(
            "SWFC",
            "customer-a",
            new PluginVersion(0, 20, 0),
            ["configuration", "documents", "database"]), Now());

        var restore = service.PrepareRestore(new RestoreRequest(
            "restore-001",
            snapshot,
            "customer-a",
            OperatorApproved: true));

        Assert.Equal(BackupRestoreStatus.Verified, snapshot.Status);
        Assert.Equal(64, snapshot.IntegrityHash.Length);
        Assert.Equal(BackupRestoreStatus.Prepared, restore.Status);
        Assert.Contains(restore.Steps, step => step.Contains("integrity hash", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void M1106_Should_Build_Product_Operations_View_From_Product_Signals()
    {
        var backup = new ProductBackupRestoreService().CreateSnapshot(new BackupScope(
            "SWFC",
            "customer-a",
            new PluginVersion(0, 20, 0),
            ["configuration"]), Now());
        var update = new ProductUpdateRolloutDecision(
            "product-update-020",
            ProductUpdateRolloutStatus.Distributed,
            ["ok"],
            ["customer-a"]);
        var operations = new ProductOperationsService();

        var view = operations.BuildView(
            "SWFC",
            "customer-a",
            [
                operations.FromBackupSnapshot(backup),
                operations.FromUpdateDecision(update)
            ]);

        Assert.Equal(ProductOperationsStatus.Operational, view.Status);
        Assert.Contains("without replacing runtime", view.Evidence, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void V020_Roadmap_And_Product_Modules_Should_Be_Marked_Done_After_Verification()
    {
        using var roadmap = JsonDocument.Parse(File.ReadAllText(
            RepositoryRoot.Combine("docs", "M600-Planning", "M601-Roadmap", "roadmap.json")));
        using var modules = JsonDocument.Parse(File.ReadAllText(
            RepositoryRoot.Combine("src", "SWFC.Web", "wwwroot", "data", "modules.json")));

        var v020 = roadmap.RootElement
            .GetProperty("Versions")
            .EnumerateArray()
            .Single(version => version.GetProperty("Version").GetString() == "v0.20.0");

        Assert.Equal("Done", v020.GetProperty("Status").GetString());
        Assert.All(v020.GetProperty("Milestones").EnumerateArray(), milestone =>
            Assert.Equal("Done", milestone.GetProperty("Status").GetString()));

        foreach (var moduleCode in new[] { "M1101", "M1102", "M1103", "M1104", "M1105", "M1106" })
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
    public void M1100_Productization_Should_Not_Contain_Business_Process_Db_Or_Placeholder_Coupling()
    {
        var files = RepositoryRoot.EnumerateFiles(
                Path.Combine("src", "SWFC.Application", "M1100-Productization-Distribution"),
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
        Assert.DoesNotContain("Authentication", combined, StringComparison.Ordinal);
    }

    private static DateTimeOffset Now() => new(2026, 4, 26, 12, 0, 0, TimeSpan.Zero);
}
