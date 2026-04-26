using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SWFC.Application;
using SWFC.Application.M1000_Platform.M1001_PluginSystem;
using SWFC.Application.M1000_Platform.M1002_DeveloperPlatformSdk;
using SWFC.Application.M1000_Platform.M1003_ExtensionManagement;
using SWFC.Application.M1000_Platform.M1004_Marketplace;
using SWFC.Application.M1000_Platform.M1005_VersioningUpdateManagement;
using SWFC.Application.M1100_ProductizationDistribution.M1101_Distribution;
using SWFC.Application.M1100_ProductizationDistribution.M1102_Updates;
using SWFC.Application.M1100_ProductizationDistribution.M1103_ProductVersioning;
using SWFC.Application.M1100_ProductizationDistribution.M1104_Licensing;
using SWFC.Application.M1100_ProductizationDistribution.M1105_BackupRestore;
using SWFC.Application.M1100_ProductizationDistribution.M1106_ProductOperations;
using SWFC.Application.M500_Runtime.M501_Scheduler;
using SWFC.Application.M500_Runtime.M502_Automation;
using SWFC.Application.M500_Runtime.M503_Job_Execution;
using SWFC.Application.M500_Runtime.M504_Control_Leitwarte;
using SWFC.Application.M500_Runtime.M505_Real_Time_Processing;
using SWFC.Application.M800_Security.M803_DataSecurity;
using SWFC.Application.M800_Security.M804_DevSecOps;
using SWFC.Application.M800_Security.M806_AccessControl.Decisions;
using SWFC.Application.M800_Security.M807_SecretsKeyManagement;
using SWFC.Application.M800_Security.M808_SecurityMonitoring;
using SWFC.Application.M800_Security.M809_CompliancePolicies;
using SWFC.Application.M900_Intelligence;
using SWFC.Application.M900_Intelligence.M901_Analytics_Intelligence_Engine;
using SWFC.Application.M900_Intelligence.M902_Prediction_Forecasting;
using SWFC.Application.M900_Intelligence.M903_Optimization;
using SWFC.Application.M900_Intelligence.M904_Anomaly_Detection;
using SWFC.Application.M900_Intelligence.M905_Intelligence_Governance;
using SWFC.Architecture.Tests.Support;
using SWFC.Infrastructure.DependencyInjection;
using SWFC.Infrastructure.M400_Integration.M401_ImportExport;
using SWFC.Infrastructure.M400_Integration.M402_API;
using SWFC.Infrastructure.M400_Integration.M403_ERPIntegration;
using SWFC.Infrastructure.M400_Integration.M404_IoTMaschinen;
using SWFC.Infrastructure.M400_Integration.M405_MessagingEvents;
using SWFC.Infrastructure.M400_Integration.M406_IdentityIntegration.Services;
using SWFC.Infrastructure.M400_Integration.M407_DMSFileIntegration;

namespace SWFC.Architecture.Tests.Rules;

public sealed class V021SystemConsolidationTests
{
    
        
    [Fact]
    public void V021_Roadmap_Should_Be_Marked_Done_After_System_Consolidation()
    {
        using var roadmap = JsonDocument.Parse(File.ReadAllText(
            RepositoryRoot.Combine("docs", "M600-Planning", "M601-Roadmap", "roadmap.json")));

        var version = roadmap.RootElement
            .GetProperty("Versions")
            .EnumerateArray()
            .Single(x => x.GetProperty("Version").GetString() == "v0.21.0");

        Assert.Equal("Done", version.GetProperty("Status").GetString());
        Assert.Equal("ALLE", version.GetProperty("RequiredModules")[0].GetString());
        Assert.All(
            version.GetProperty("Milestones").EnumerateArray(),
            milestone => Assert.Equal("Done", milestone.GetProperty("Status").GetString()));
    }

    [Fact]
    public void All_Planned_Modules_Should_Remain_Fully_Completed_In_Module_Catalog()
    {
        using var modules = JsonDocument.Parse(File.ReadAllText(
            RepositoryRoot.Combine("src", "SWFC.Web", "wwwroot", "data", "modules.json")));

        var moduleEntries = modules.RootElement
            .GetProperty("Groups")
            .EnumerateArray()
            .SelectMany(group => group.GetProperty("Modules").EnumerateArray())
            .ToArray();

        Assert.NotEmpty(moduleEntries);
        Assert.All(moduleEntries, module =>
        {
            Assert.Equal("Full Complete", module.GetProperty("Status").GetString());
            Assert.Equal(100, module.GetProperty("ProgressPercent").GetInt32());
            Assert.All(
                module.GetProperty("WorkItems").EnumerateArray(),
                item => Assert.Equal("Done", item.GetProperty("Status").GetString()));
        });
    }

    [Fact]
    public async Task Cross_Module_Services_Should_Be_Resolvable_And_Keep_Handoffs_Explicit()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHttpContextAccessor();
        services.AddApplication();
        services.AddInfrastructure(BuildConfiguration());

        await using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true
        });
        using var scope = provider.CreateScope();
        var scoped = scope.ServiceProvider;

        Assert.NotNull(scoped.GetRequiredService<StructuredDataExchangeAdapter>());
        Assert.NotNull(scoped.GetRequiredService<IntegrationApiGateway>());
        Assert.NotNull(scoped.GetRequiredService<IntegrationEventDispatcher>());
        Assert.NotNull(scoped.GetRequiredService<OidcProviderRegistry>());
        Assert.NotNull(scoped.GetRequiredService<OidcExternalIdentityResolver>());

        Assert.NotNull(scoped.GetRequiredService<IRuntimeScheduler>());
        Assert.NotNull(scoped.GetRequiredService<IAutomationRuleEngine>());
        Assert.NotNull(scoped.GetRequiredService<IRuntimeJobExecutor>());
        Assert.NotNull(scoped.GetRequiredService<IControlDeskRuntime>());
        Assert.NotNull(scoped.GetRequiredService<IRealTimeProcessor>());

        Assert.NotNull(scoped.GetRequiredService<IDataProtectionService>());
        Assert.NotNull(scoped.GetRequiredService<ISecurityReleaseGate>());
        Assert.NotNull(scoped.GetRequiredService<IAccessDecisionService>());
        Assert.NotNull(scoped.GetRequiredService<ISecretVaultService>());
        Assert.NotNull(scoped.GetRequiredService<ISecurityMonitoringService>());
        Assert.NotNull(scoped.GetRequiredService<ISecurityPolicyService>());

        Assert.NotNull(scoped.GetRequiredService<AnalyticsIntelligenceEngine>());
        Assert.NotNull(scoped.GetRequiredService<PredictionForecastingService>());
        Assert.NotNull(scoped.GetRequiredService<OptimizationService>());
        Assert.NotNull(scoped.GetRequiredService<AnomalyDetectionService>());
        Assert.NotNull(scoped.GetRequiredService<IntelligenceGovernanceService>());
        Assert.NotNull(scoped.GetRequiredService<IntelligenceWorkspaceService>());

        Assert.NotNull(scoped.GetRequiredService<IPluginCatalog>());
        Assert.NotNull(scoped.GetRequiredService<IDeveloperPlatformSdk>());
        Assert.NotNull(scoped.GetRequiredService<IExtensionLifecycleManager>());
        Assert.NotNull(scoped.GetRequiredService<IMarketplaceCatalog>());
        Assert.NotNull(scoped.GetRequiredService<IPlatformVersioningService>());
        Assert.NotNull(scoped.GetRequiredService<IProductDistributionService>());
        Assert.NotNull(scoped.GetRequiredService<IProductUpdateOrchestrator>());
        Assert.NotNull(scoped.GetRequiredService<IProductVersionRegistry>());
        Assert.NotNull(scoped.GetRequiredService<IProductLicensingService>());
        Assert.NotNull(scoped.GetRequiredService<IProductBackupRestoreService>());
        Assert.NotNull(scoped.GetRequiredService<IProductOperationsService>());

        await VerifyM403ErpHandoffAsync(scoped);
        await VerifyM404TelemetryHandoffAsync(scoped);
        await VerifyM407DocumentReferenceHandoffAsync(scoped);
    }

    private static IConfiguration BuildConfiguration() =>
    new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Security:DataProtection:Key"] = "v021-consolidation-test-key",
            ["Security:DataProtection:KeyVersion"] = "v021"
        })
        .Build();

    private static async Task VerifyM403ErpHandoffAsync(IServiceProvider services)
    {
        var adapter = services.GetRequiredService<ErpIntegrationAdapter>();
        var transport = Assert.IsType<InProcessErpTransportAdapter>(
            services.GetRequiredService<IErpTransportAdapter>());

        var message = await adapter.TransferPurchaseOrderAsync(
            "SAP",
            new ErpPurchaseOrderTransfer(
                Guid.NewGuid(),
                "PO-V021",
                Guid.NewGuid(),
                ExistingErpReference: null,
                DocumentReference: "M104:PurchaseOrder:PO-V021"));

        Assert.Equal("M206", message.Payload["SourceModule"]);
        Assert.Contains(transport.Messages, x => x.CorrelationId == message.CorrelationId);
    }

    private static async Task VerifyM404TelemetryHandoffAsync(IServiceProvider services)
    {
        var adapter = services.GetRequiredService<MachineTelemetryAdapter>();
        var sink = Assert.IsType<InProcessMachineTelemetrySink>(
            services.GetRequiredService<IMachineTelemetrySink>());

        var envelope = await adapter.TransferAsync(new MachineTelemetryPacket(
            "PCS",
            "M-V021",
            DateTime.UtcNow,
            [new MachineTelemetryPoint("energy.active", MachineTelemetryKind.Measurement, "42.5", "kWh", "M205")],
            new Dictionary<string, string?> { ["line"] = "A" }));

        Assert.Contains("M205", envelope.SupportedConsumerModules);
        Assert.Contains(sink.Envelopes, x => x.ExternalMachineKey == "M-V021");
    }

    private static async Task VerifyM407DocumentReferenceHandoffAsync(IServiceProvider services)
    {
        var adapter = services.GetRequiredService<DmsFileReferenceAdapter>();
        var sink = Assert.IsType<InProcessDocumentReferenceSink>(
            services.GetRequiredService<IDocumentReferenceSink>());

        var reference = await adapter.LinkAsync(new DmsExternalDocumentRequest(
            "SharePoint",
            "DOC-V021",
            new Uri("https://dms.example.test/docs/DOC-V021"),
            "v021.pdf",
            "application/pdf",
            512,
            "M104",
            "Document",
            "document-v021"));

        Assert.Equal("M104:Document:document-v021:DOC-V021", reference.SwfcReferenceKey);
        Assert.Contains(sink.References, x => x.SwfcReferenceKey == reference.SwfcReferenceKey);
    }
}
