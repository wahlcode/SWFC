using System.Xml.Linq;
using SWFC.Architecture.Tests.Support;

namespace SWFC.Architecture.Tests.Rules;

public sealed class DocumentationDrivenArchitectureTests
{
    [Fact]
    public void Source_Projects_Should_Respect_Documented_Layer_References()
    {
        var domainReferences = GetProjectReferences("src", "SWFC.Domain", "SWFC.Domain.csproj");
        var applicationReferences = GetProjectReferences("src", "SWFC.Application", "SWFC.Application.csproj");
        var infrastructureReferences = GetProjectReferences("src", "SWFC.Infrastructure", "SWFC.Infrastructure.csproj");
        var webReferences = GetProjectReferences("src", "SWFC.Web", "SWFC.Web.csproj");

        Assert.DoesNotContain("SWFC.Application", domainReferences);
        Assert.DoesNotContain("SWFC.Infrastructure", domainReferences);
        Assert.DoesNotContain("SWFC.Web", domainReferences);

        Assert.Contains("SWFC.Domain", applicationReferences);
        Assert.DoesNotContain("SWFC.Infrastructure", applicationReferences);
        Assert.DoesNotContain("SWFC.Web", applicationReferences);

        Assert.Contains("SWFC.Domain", infrastructureReferences);
        Assert.Contains("SWFC.Application", infrastructureReferences);
        Assert.DoesNotContain("SWFC.Web", infrastructureReferences);

        Assert.DoesNotContain("SWFC.Domain", webReferences);
    }

    [Fact]
    public void Web_Should_Not_Use_Forbidden_External_Ui_Frameworks()
    {
        var forbiddenFrameworks = new[] { "MudBlazor", "Telerik", "Syncfusion" };
        var sourceFiles = Directory.EnumerateFiles(
            RepositoryRoot.Combine("src", "SWFC.Web"),
            "*.*",
            SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{System.IO.Path.DirectorySeparatorChar}bin{System.IO.Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains($"{System.IO.Path.DirectorySeparatorChar}obj{System.IO.Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        var matches = sourceFiles
            .Where(path => path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".razor", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, index) => new { path, line, lineNumber = index + 1 }))
            .Where(entry => forbiddenFrameworks.Any(framework =>
                entry.line.Contains(framework, StringComparison.OrdinalIgnoreCase)))
            .Select(entry => $"{entry.path}:{entry.lineNumber}")
            .ToArray();

        Assert.True(
            matches.Length == 0,
            $"Forbidden UI framework references found: {string.Join(", ", matches)}");
    }

    [Fact]
    public void Razor_Components_Should_Not_Use_Inline_Styles()
    {
        var offendingFiles = Directory.EnumerateFiles(
                RepositoryRoot.Combine("src", "SWFC.Web"),
                "*.razor",
                SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{System.IO.Path.DirectorySeparatorChar}bin{System.IO.Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains($"{System.IO.Path.DirectorySeparatorChar}obj{System.IO.Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(path => File.ReadLines(path)
                .Any(line => line.Contains("style=", StringComparison.OrdinalIgnoreCase)))
            .ToArray();

        Assert.True(
            offendingFiles.Length == 0,
            $"Inline styles are forbidden by the architecture rules: {string.Join(", ", offendingFiles)}");
    }

    private static IReadOnlyCollection<string> GetProjectReferences(params string[] pathSegments)
    {
        var projectPath = RepositoryRoot.Combine(pathSegments);
        var document = XDocument.Load(projectPath);

        return document
            .Descendants()
            .Where(element => element.Name.LocalName == "ProjectReference")
            .Select(element => element.Attribute("Include")?.Value)
            .Where(include => !string.IsNullOrWhiteSpace(include))
            .Select(include => System.IO.Path.GetFileNameWithoutExtension(include!))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
