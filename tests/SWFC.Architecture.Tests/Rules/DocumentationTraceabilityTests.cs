using SWFC.Architecture.Tests.Support;

namespace SWFC.Architecture.Tests.Rules;

public sealed class DocumentationTraceabilityTests
{
    [Fact]
    public void Documented_Module_Groups_Should_Be_Traceable_To_Source_Areas()
    {
        var sourceDirectoryNames = RepositoryRoot.EnumerateDirectories("src")
            .Select(System.IO.Path.GetFileName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Cast<string>()
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missingGroups = DocumentationCatalog.ModuleGroupFiles
            .Where(doc => doc.ModuleId is not null)
            .Select(doc => new
            {
                doc.RelativePath,
                doc.ModuleId,
                IsBackedBySource = HasSourceBacking(doc.ModuleId!, sourceDirectoryNames)
            })
            .Where(result => !result.IsBackedBySource)
            .Select(result => $"{result.ModuleId} ({result.RelativePath})")
            .ToArray();

        Assert.True(
            missingGroups.Length == 0,
            $"Documented module groups without source-area backing: {string.Join(", ", missingGroups)}");
    }

    [Fact]
    public void Documented_Module_Details_Should_Be_Traceable_To_Source_Module_Directories()
    {
        var implementedModuleIds = RepositoryRoot.GetImplementedModuleIds(
            System.IO.Path.Combine("src", "SWFC.Domain"),
            System.IO.Path.Combine("src", "SWFC.Application"),
            System.IO.Path.Combine("src", "SWFC.Infrastructure"),
            System.IO.Path.Combine("src", "SWFC.Web"));

        var missingModules = DocumentationCatalog.ModuleDetailFiles
            .Where(doc => doc.ModuleId is not null)
            .Select(doc => new { doc.RelativePath, doc.ModuleId })
            .Where(result => !implementedModuleIds.Contains(result.ModuleId!, StringComparer.OrdinalIgnoreCase))
            .Select(result => $"{result.ModuleId} ({result.RelativePath})")
            .ToArray();

        Assert.True(
            missingModules.Length == 0,
            $"Documented modules without source implementation: {string.Join(", ", missingModules)}");
    }

    private static bool HasSourceBacking(string moduleGroupId, IReadOnlySet<string> sourceDirectoryNames)
    {
        return moduleGroupId switch
        {
            "M100" => sourceDirectoryNames.Contains("M100-System"),
            "M200" => sourceDirectoryNames.Contains("M200-Business"),
            "M300" => sourceDirectoryNames.Contains("M300-Presentation"),
            "M400" => sourceDirectoryNames.Contains("M400-Integration"),
            "M500" => sourceDirectoryNames.Contains("M500-Runtime") || sourceDirectoryNames.Contains("SWFC.Worker"),
            "M600" => sourceDirectoryNames.Contains("M600-Planning"),
            "M700" => sourceDirectoryNames.Contains("M700-Support"),
            "M800" => sourceDirectoryNames.Contains("M800-Security"),
            "M900" => sourceDirectoryNames.Contains("M900-Intelligence"),
            "M1000" => sourceDirectoryNames.Contains("M1000-Platform"),
            "M1100" => sourceDirectoryNames.Contains("M1100-Productization-Distribution"),
            _ => false
        };
    }
}
