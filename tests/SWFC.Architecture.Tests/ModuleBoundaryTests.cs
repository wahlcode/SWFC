using NetArchTest.Rules;
using Xunit;

namespace SWFC.Architecture.Tests;

public class ModuleBoundaryTests
{
    private static readonly (string ModuleName, string NamespacePrefix)[] DomainModules =
    {
        ("M101-Foundation", "SWFC.Domain.M100_System.M101_Foundation"),
        ("M102-Organization", "SWFC.Domain.M100_System.M102_Organization"),
        ("M103-Security", "SWFC.Domain.M100_System.M103_Security"),
        ("M104-Documents", "SWFC.Domain.M100_System.M104_Documents"),
        ("M105-Configuration", "SWFC.Domain.M100_System.M105_Configuration"),
        ("M106-Theme", "SWFC.Domain.M100_System.M106_Theme"),

        ("M201-Assets", "SWFC.Domain.M200_Business.M201_Assets"),
        ("M202-Maintenance", "SWFC.Domain.M200_Business.M202_Maintenance"),
        ("M203-Inspections", "SWFC.Domain.M200_Business.M203_Inspections"),
        ("M204-Inventory", "SWFC.Domain.M200_Business.M204_Inventory"),
        ("M205-Energy", "SWFC.Domain.M200_Business.M205_Energy"),
        ("M206-Purchasing", "SWFC.Domain.M200_Business.M206_Purchasing"),
        ("M207-Quality", "SWFC.Domain.M200_Business.M207_Quality"),
        ("M208-Safety", "SWFC.Domain.M200_Business.M208_Safety"),
        ("M209-Projects", "SWFC.Domain.M200_Business.M209_Projects"),
        ("M210-Customers", "SWFC.Domain.M200_Business.M210_Customers"),
        ("M211-Analytics", "SWFC.Domain.M200_Business.M211_Analytics"),

        ("M301-Dashboard", "SWFC.Domain.M300_Presentation.M301_Dashboard"),
        ("M302-Reporting", "SWFC.Domain.M300_Presentation.M302_Reporting"),
        ("M303-Notifications", "SWFC.Domain.M300_Presentation.M303_Notifications"),
        ("M304-Search", "SWFC.Domain.M300_Presentation.M304_Search"),
        ("M305-AppShell", "SWFC.Domain.M300_Presentation.M305_AppShell"),

        ("M401-ImportExport", "SWFC.Domain.M400_Integrations.M401_ImportExport"),
        ("M402-API", "SWFC.Domain.M400_Integrations.M402_API"),
        ("M403-ERP", "SWFC.Domain.M400_Integrations.M403_ERP"),
        ("M404-IoT", "SWFC.Domain.M400_Integrations.M404_IoT"),

        ("M501-Scheduler", "SWFC.Domain.M500_Runtime.M501_Scheduler"),
        ("M502-Communication", "SWFC.Domain.M500_Runtime.M502_Communication"),
        ("M503-Automation", "SWFC.Domain.M500_Runtime.M503_Automation"),
        ("M504-Updater", "SWFC.Domain.M500_Runtime.M504_Updater"),
        ("M505-Versioning", "SWFC.Domain.M500_Runtime.M505_Versioning"),

        ("M601-Roadmap", "SWFC.Domain.M600_Planning.M601_Roadmap"),
        ("M602-Concepts", "SWFC.Domain.M600_Planning.M602_Concepts"),
        ("M603-Prototypes", "SWFC.Domain.M600_Planning.M603_Prototypes"),

        ("M701-BugTracking", "SWFC.Domain.M700_Support.M701_BugTracking"),
        ("M702-ChangeRequests", "SWFC.Domain.M700_Support.M702_ChangeRequests"),
        ("M703-SupportCases", "SWFC.Domain.M700_Support.M703_SupportCases"),

        ("M801-SecurityFoundation", "SWFC.Domain.M800_Security.M801_SecurityFoundation"),
        ("M802-ApplicationSecurity", "SWFC.Domain.M800_Security.M802_ApplicationSecurity"),
        ("M803-DataSecurity", "SWFC.Domain.M800_Security.M803_DataSecurity"),
        ("M804-DevSecOps", "SWFC.Domain.M800_Security.M804_DevSecOps"),
        ("M805-AuditCompliance", "SWFC.Domain.M800_Security.M805_AuditCompliance"),

        ("M901-AI", "SWFC.Domain.M900_Intelligence.M901_AI"),
        ("M902-Monitoring", "SWFC.Domain.M900_Intelligence.M902_Monitoring"),
        ("M903-Incident", "SWFC.Domain.M900_Intelligence.M903_Incident"),
        ("M904-IntegrationGovernance", "SWFC.Domain.M900_Intelligence.M904_IntegrationGovernance"),
        ("M905-RuleEngine", "SWFC.Domain.M900_Intelligence.M905_RuleEngine"),

        ("M1001-PluginSystem", "SWFC.Domain.M1000_Platform.M1001_PluginSystem"),
        ("M1002-DeveloperPlatform", "SWFC.Domain.M1000_Platform.M1002_DeveloperPlatform"),
        ("M1003-ExtensionManagement", "SWFC.Domain.M1000_Platform.M1003_ExtensionManagement"),
        ("M1004-Marketplace", "SWFC.Domain.M1000_Platform.M1004_Marketplace"),
        ("M1005-ExternalSecurity", "SWFC.Domain.M1000_Platform.M1005_ExternalSecurity")
    };

    private static readonly (string ModuleName, string NamespacePrefix)[] ApplicationModules =
    {
        ("M100-System", "SWFC.Application.M100_System"),
        ("M200-Business", "SWFC.Application.M200_Business"),
        ("M300-Presentation", "SWFC.Application.M300_Presentation"),
        ("M400-Integrations", "SWFC.Application.M400_Integrations"),
        ("M500-Runtime", "SWFC.Application.M500_Runtime"),
        ("M600-Planning", "SWFC.Application.M600_Planning"),
        ("M700-Support", "SWFC.Application.M700_Support"),
        ("M800-Security", "SWFC.Application.M800_Security"),
        ("M900-Intelligence", "SWFC.Application.M900_Intelligence"),
        ("M1000-Platform", "SWFC.Application.M1000_Platform")
    };

    [Fact]
    public void Domain_Modules_Should_Not_Depend_On_Other_Domain_Modules()
    {
        foreach (var source in DomainModules)
        {
            var sourceTypes = Types.InAssembly(typeof(SWFC.Domain.AssemblyMarker).Assembly)
                .That()
                .ResideInNamespace(source.NamespacePrefix);

            foreach (var target in DomainModules)
            {
                if (source.NamespacePrefix == target.NamespacePrefix)
                {
                    continue;
                }

                var result = sourceTypes
                    .ShouldNot()
                    .HaveDependencyOn(target.NamespacePrefix)
                    .GetResult();

                Assert.True(
                    result.IsSuccessful,
                    $"{source.ModuleName} darf nicht von {target.ModuleName} abhängen.");
            }
        }
    }

    [Fact]
    public void Application_Module_Groups_Should_Not_Depend_On_Other_Application_Module_Groups()
    {
        const string securityModule = "SWFC.Application.M800_Security";

        foreach (var source in ApplicationModules)
        {
            // M800-Security wird separat im eigenen Test geprüft
            if (source.NamespacePrefix == securityModule)
            {
                continue;
            }

            var sourceTypes = Types.InAssembly(typeof(SWFC.Application.AssemblyMarker).Assembly)
                .That()
                .ResideInNamespace(source.NamespacePrefix);

            foreach (var target in ApplicationModules)
            {
                if (source.NamespacePrefix == target.NamespacePrefix)
                {
                    continue;
                }

                // Erlaubte Ausnahme:
                // Alle anderen Modulgruppen dürfen M800-Security verwenden.
                if (target.NamespacePrefix == securityModule)
                {
                    continue;
                }

                var result = sourceTypes
                    .ShouldNot()
                    .HaveDependencyOn(target.NamespacePrefix)
                    .GetResult();

                Assert.True(
                    result.IsSuccessful,
                    $"{source.ModuleName} darf nicht von {target.ModuleName} abhängen.");
            }
        }
    }

    [Fact]
    public void M800_Security_Should_Not_Depend_On_Other_Application_Module_Groups()
    {
        const string securityModule = "SWFC.Application.M800_Security";

        var securityTypes = Types.InAssembly(typeof(SWFC.Application.AssemblyMarker).Assembly)
            .That()
            .ResideInNamespace(securityModule);

        foreach (var target in ApplicationModules)
        {
            if (target.NamespacePrefix == securityModule)
            {
                continue;
            }

            var result = securityTypes
                .ShouldNot()
                .HaveDependencyOn(target.NamespacePrefix)
                .GetResult();

            Assert.True(
                result.IsSuccessful,
                $"M800-Security darf nicht von {target.ModuleName} abhängen.");
        }
    }
}