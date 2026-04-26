using Microsoft.Extensions.DependencyInjection;
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
using SWFC.Application.M900_Intelligence;
using SWFC.Application.M900_Intelligence.M901_Analytics_Intelligence_Engine;
using SWFC.Application.M900_Intelligence.M902_Prediction_Forecasting;
using SWFC.Application.M900_Intelligence.M903_Optimization;
using SWFC.Application.M900_Intelligence.M904_Anomaly_Detection;
using SWFC.Application.M900_Intelligence.M905_Intelligence_Governance;
using SWFC.Application.M800_Security.M803_DataSecurity;
using SWFC.Application.M800_Security.M804_DevSecOps;
using SWFC.Application.M800_Security.M807_SecretsKeyManagement;
using SWFC.Application.M800_Security.M808_SecurityMonitoring;
using SWFC.Application.M800_Security.M809_CompliancePolicies;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Configuration;
using SWFC.Application.M800_Security.M806_AccessControl.Decisions;

namespace SWFC.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationService, AuthorizationService>();
        services.AddScoped<IAccessDecisionService, AccessDecisionService>();
        services.AddScoped<IDataProtectionService, DataProtectionService>();
        services.AddScoped<ISecurityReleaseGate, SecurityReleaseGate>();
        services.AddScoped<ISecretVaultService, SecretVaultService>();
        services.AddScoped<ISecurityMonitoringService, SecurityMonitoringService>();
        services.AddScoped<ISecurityPolicyService, SecurityPolicyService>();
        services.AddM802PipelineEnforcement();
        services.AddScoped<IPluginCatalog, PluginCatalog>();
        services.AddScoped<IDeveloperPlatformSdk, DeveloperPlatformSdk>();
        services.AddScoped<IExtensionLifecycleManager, ExtensionLifecycleManager>();
        services.AddScoped<IMarketplaceCatalog, MarketplaceCatalog>();
        services.AddScoped<IPlatformVersioningService, PlatformVersioningService>();
        services.AddScoped<IProductDistributionService, ProductDistributionService>();
        services.AddScoped<IProductUpdateOrchestrator, ProductUpdateOrchestrator>();
        services.AddScoped<IProductVersionRegistry, ProductVersionRegistry>();
        services.AddScoped<IProductLicensingService, ProductLicensingService>();
        services.AddScoped<IProductBackupRestoreService, ProductBackupRestoreService>();
        services.AddScoped<IProductOperationsService, ProductOperationsService>();
        services.AddScoped<IRuntimeScheduler, RuntimeScheduler>();
        services.AddScoped<IAutomationRuleEngine, AutomationRuleEngine>();
        services.AddScoped<IRuntimeJobExecutor, RuntimeJobExecutor>();
        services.AddScoped<IControlDeskRuntime, ControlDeskRuntime>();
        services.AddScoped<IRealTimeProcessor, RealTimeProcessor>();
        services.AddScoped<AnalyticsIntelligenceEngine>();
        services.AddScoped<PredictionForecastingService>();
        services.AddScoped<OptimizationService>();
        services.AddScoped<AnomalyDetectionService>();
        services.AddScoped<IntelligenceGovernanceService>();
        services.AddScoped<IntelligenceWorkspaceService>();

        return services;
    }
}

