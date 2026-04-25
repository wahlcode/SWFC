using SWFC.Architecture.Tests.Support;

namespace SWFC.Architecture.Tests.Documentation;

public sealed class DocumentationInventoryTests
{
    [Fact]
    public void All_Discovered_Documentation_Files_Should_Have_Content_And_A_Primary_Heading()
    {
        var violations = DocumentationCatalog.All
            .Select(doc =>
            {
                var firstLine = DocumentationCatalog.GetFirstMeaningfulLine(doc.AbsolutePath);
                var normalized = DocumentationCatalog.NormalizeMarkdownLine(firstLine);

                return new
                {
                    doc.RelativePath,
                    HasContent = !string.IsNullOrWhiteSpace(firstLine),
                    HasHeading = normalized.StartsWith("#", StringComparison.Ordinal)
                };
            })
            .Where(result => !result.HasContent || !result.HasHeading)
            .Select(result => result.RelativePath)
            .ToArray();

        Assert.True(
            violations.Length == 0,
            $"Markdown files without usable content or heading: {string.Join(", ", violations)}");
    }

    [Fact]
    public void All_Discovered_Documentation_Files_Should_Be_Mapped_To_At_Least_One_Automated_Rule_Family()
    {
        var uncoveredFiles = DocumentationCatalog.All
            .Where(doc => doc.RuleFamilies.Count == 0)
            .Select(doc => doc.RelativePath)
            .ToArray();

        Assert.True(
            uncoveredFiles.Length == 0,
            $"Documentation files without automated rule coverage: {string.Join(", ", uncoveredFiles)}");
    }

    [Fact]
    public void Module_Documentation_Files_Should_Expose_Module_Ids_That_Match_File_Name_And_Heading()
    {
        var violations = DocumentationCatalog.ModuleGroupFiles
            .Concat(DocumentationCatalog.ModuleDetailFiles)
            .Select(doc => new
            {
                doc.RelativePath,
                FileModuleId = doc.ModuleId,
                HeadingModuleId = DocumentationCatalog.ExtractModuleIdFromHeading(
                    DocumentationCatalog.GetFirstMeaningfulLine(doc.AbsolutePath))
            })
            .Where(result => string.IsNullOrWhiteSpace(result.FileModuleId)
                || !string.Equals(result.FileModuleId, result.HeadingModuleId, StringComparison.OrdinalIgnoreCase))
            .Select(result => result.RelativePath)
            .ToArray();

        Assert.True(
            violations.Length == 0,
            $"Module docs whose heading does not match the file module id: {string.Join(", ", violations)}");
    }
}
