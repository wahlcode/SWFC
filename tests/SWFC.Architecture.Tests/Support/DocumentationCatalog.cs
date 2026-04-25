using System.Text.RegularExpressions;

namespace SWFC.Architecture.Tests.Support;

internal enum DocumentationKind
{
    Root,
    ModuleGroup,
    ModuleDetail
}

internal sealed record DocumentationFile(
    string AbsolutePath,
    string RelativePath,
    string FileNameWithoutExtension,
    string? ModuleId,
    DocumentationKind Kind,
    IReadOnlyCollection<string> RuleFamilies);

internal static class DocumentationCatalog
{
    private static readonly Regex ModuleIdPattern = new(@"^(M\d{3,4})[_-]", RegexOptions.Compiled);
    private static readonly Regex HeadingModuleIdPattern = new(@"^#+\s*(M\d{3,4})\b", RegexOptions.Compiled);

    private static readonly IReadOnlyDictionary<string, string[]> RootRuleFamilies =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["00_System_Overview.md"] = ["documentation-integrity", "system-overview", "codebase-audit"],
            ["01_Modulstruktur.md"] = ["documentation-integrity", "module-structure", "traceability"],
            ["02_Architektur_Regeln.md"] = ["documentation-integrity", "layer-architecture", "ui-boundaries"],
            ["03_Master_Regelwerk.md"] = ["documentation-integrity", "master-rules", "code-patterns"],
            ["04_Ordnerstruktur.md"] = ["documentation-integrity", "repository-structure", "theme-layout", "worker-presence"],
            ["05_Naming_Conventions.md"] = ["documentation-integrity", "naming-conventions", "code-patterns"]
        };

    public static IReadOnlyCollection<DocumentationFile> All { get; } = Load();

    public static IEnumerable<DocumentationFile> RootFiles => All.Where(x => x.Kind == DocumentationKind.Root);

    public static IEnumerable<DocumentationFile> ModuleGroupFiles => All.Where(x => x.Kind == DocumentationKind.ModuleGroup);

    public static IEnumerable<DocumentationFile> ModuleDetailFiles => All.Where(x => x.Kind == DocumentationKind.ModuleDetail);

    public static string? GetFirstMeaningfulLine(string absolutePath)
    {
        return File.ReadLines(absolutePath)
            .Select(line => line.Trim())
            .FirstOrDefault(line => !string.IsNullOrWhiteSpace(line));
    }

    public static string NormalizeMarkdownLine(string? line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return string.Empty;
        }

        var normalized = line.TrimStart();

        while (normalized.StartsWith("\\", StringComparison.Ordinal))
        {
            normalized = normalized[1..].TrimStart();
        }

        return normalized;
    }

    public static string? ExtractModuleIdFromHeading(string? headingLine)
    {
        var normalized = NormalizeMarkdownLine(headingLine);
        var match = HeadingModuleIdPattern.Match(normalized);

        return match.Success ? match.Groups[1].Value : null;
    }

    private static IReadOnlyCollection<DocumentationFile> Load()
    {
        return RepositoryRoot.EnumerateFiles("docs", "*.md")
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .Select(CreateDocumentationFile)
            .ToArray();
    }

    private static DocumentationFile CreateDocumentationFile(string absolutePath)
    {
        var relativePath = RepositoryRoot.ToRelativePath(absolutePath);
        var fileName = System.IO.Path.GetFileName(absolutePath);
        var fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(absolutePath);
        var moduleId = ExtractModuleId(fileNameWithoutExtension);
        var kind = GetDocumentationKind(relativePath, fileNameWithoutExtension);
        var ruleFamilies = GetRuleFamilies(fileName, kind);

        return new DocumentationFile(
            absolutePath,
            relativePath,
            fileNameWithoutExtension,
            moduleId,
            kind,
            ruleFamilies);
    }

    private static DocumentationKind GetDocumentationKind(string relativePath, string fileNameWithoutExtension)
    {
        var normalizedRelativePath = relativePath.Replace(System.IO.Path.AltDirectorySeparatorChar, System.IO.Path.DirectorySeparatorChar);
        var modulesPrefix = $"docs{System.IO.Path.DirectorySeparatorChar}modules{System.IO.Path.DirectorySeparatorChar}";

        if (!normalizedRelativePath.StartsWith(modulesPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return DocumentationKind.Root;
        }

        var directoryName = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(normalizedRelativePath)!);

        return string.Equals(directoryName, fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase)
            ? DocumentationKind.ModuleGroup
            : DocumentationKind.ModuleDetail;
    }

    private static string[] GetRuleFamilies(string fileName, DocumentationKind kind)
    {
        if (kind == DocumentationKind.Root && RootRuleFamilies.TryGetValue(fileName, out var families))
        {
            return families;
        }

        return kind switch
        {
            DocumentationKind.Root => ["documentation-integrity"],
            DocumentationKind.ModuleGroup => ["documentation-integrity", "module-group-traceability"],
            DocumentationKind.ModuleDetail => ["documentation-integrity", "module-detail-traceability"],
            _ => ["documentation-integrity"]
        };
    }

    private static string? ExtractModuleId(string fileNameWithoutExtension)
    {
        var match = ModuleIdPattern.Match(fileNameWithoutExtension);
        return match.Success ? match.Groups[1].Value : null;
    }
}
