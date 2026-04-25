using SWFC.Architecture.Tests.Support;

namespace SWFC.Architecture.Tests.Structure;

public sealed class RepositoryStructureTests
{
    [Fact]
    public void Repository_Should_Contain_Documented_Core_Project_Folders()
    {
        var requiredDirectories = new[]
        {
            RepositoryRoot.Combine("src"),
            RepositoryRoot.Combine("docs"),
            RepositoryRoot.Combine("src", "SWFC.Domain"),
            RepositoryRoot.Combine("src", "SWFC.Application"),
            RepositoryRoot.Combine("src", "SWFC.Infrastructure"),
            RepositoryRoot.Combine("src", "SWFC.Web"),
            RepositoryRoot.Combine("src", "SWFC.Worker")
        };

        var missingDirectories = requiredDirectories
            .Where(path => !Directory.Exists(path))
            .ToArray();

        Assert.True(
            missingDirectories.Length == 0,
            $"Documented directories are missing: {string.Join(", ", missingDirectories)}");
    }

    [Fact]
    public void Web_Should_Keep_Documented_Theme_And_Layout_Files()
    {
        var requiredFiles = new[]
        {
            RepositoryRoot.Combine("src", "SWFC.Web", "wwwroot", "css", "theme.css"),
            RepositoryRoot.Combine("src", "SWFC.Web", "Components", "Layout", "AppShell.razor"),
            RepositoryRoot.Combine("src", "SWFC.Web", "Components", "Layout", "Header.razor"),
            RepositoryRoot.Combine("src", "SWFC.Web", "Components", "Layout", "Sidebar.razor"),
            RepositoryRoot.Combine("src", "SWFC.Web", "Components", "Layout", "MainLayout.razor")
        };

        var missingFiles = requiredFiles
            .Where(path => !File.Exists(path))
            .ToArray();

        Assert.True(
            missingFiles.Length == 0,
            $"Documented layout files are missing: {string.Join(", ", missingFiles)}");
    }

    [Fact]
    public void Implemented_Module_Ids_Should_Be_Covered_By_Module_Documentation()
    {
        var documentedModuleIds = RepositoryRoot.GetDocumentedModuleIds();
        var implementedModuleIds = RepositoryRoot.GetImplementedModuleIds(
            System.IO.Path.Combine("src", "SWFC.Domain"),
            System.IO.Path.Combine("src", "SWFC.Application"),
            System.IO.Path.Combine("src", "SWFC.Web"));

        var undocumentedModuleIds = implementedModuleIds
            .Except(documentedModuleIds, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        Assert.True(
            undocumentedModuleIds.Length == 0,
            $"Implemented module IDs missing from docs/modules: {string.Join(", ", undocumentedModuleIds)}");
    }
}
