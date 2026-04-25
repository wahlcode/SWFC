using System.Text.RegularExpressions;

namespace SWFC.Architecture.Tests.Support;

internal static class RepositoryRoot
{
    private static readonly Regex HyphenatedModulePattern = new(@"^(M\d{3,4})-", RegexOptions.Compiled);
    private static readonly Regex DocumentationModulePattern = new(@"^(M\d{3,4})[_-]", RegexOptions.Compiled);

    public static string Path { get; } = FindRepositoryRoot();

    public static string Combine(params string[] segments)
    {
        return segments.Aggregate(Path, System.IO.Path.Combine);
    }

    public static string ToRelativePath(string absolutePath)
    {
        return System.IO.Path.GetRelativePath(Path, absolutePath);
    }

    public static IEnumerable<string> EnumerateFiles(
        string relativeRoot,
        string searchPattern,
        SearchOption searchOption = SearchOption.AllDirectories,
        bool includeArtifactDirectories = false,
        bool includeGeneratedFiles = true)
    {
        var absoluteRoot = Combine(relativeRoot);
        if (!Directory.Exists(absoluteRoot))
        {
            return Array.Empty<string>();
        }

        return Directory.EnumerateFiles(absoluteRoot, searchPattern, searchOption)
            .Where(path => includeArtifactDirectories || !IsArtifactPath(path))
            .Where(path => includeGeneratedFiles || !IsGeneratedOrThirdPartyPath(path));
    }

    public static IEnumerable<string> EnumerateDirectories(
        string relativeRoot,
        string searchPattern = "*",
        SearchOption searchOption = SearchOption.AllDirectories,
        bool includeArtifactDirectories = false)
    {
        var absoluteRoot = Combine(relativeRoot);
        if (!Directory.Exists(absoluteRoot))
        {
            return Array.Empty<string>();
        }

        return Directory.EnumerateDirectories(absoluteRoot, searchPattern, searchOption)
            .Where(path => includeArtifactDirectories || !IsArtifactPath(path));
    }

    public static bool IsArtifactPath(string path)
    {
        var normalized = NormalizePath(path);

        return normalized.Contains($"{System.IO.Path.DirectorySeparatorChar}bin{System.IO.Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains($"{System.IO.Path.DirectorySeparatorChar}obj{System.IO.Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsGeneratedOrThirdPartyPath(string path)
    {
        var normalized = NormalizePath(path);
        var fileName = System.IO.Path.GetFileName(path);

        return IsArtifactPath(path)
            || normalized.Contains($"{System.IO.Path.DirectorySeparatorChar}wwwroot{System.IO.Path.DirectorySeparatorChar}lib{System.IO.Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains($"{System.IO.Path.DirectorySeparatorChar}Persistence{System.IO.Path.DirectorySeparatorChar}Migrations{System.IO.Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
            || fileName.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase)
            || fileName.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase)
            || fileName.EndsWith(".generated.cs", StringComparison.OrdinalIgnoreCase)
            || fileName.EndsWith(".AssemblyInfo.cs", StringComparison.OrdinalIgnoreCase)
            || fileName.EndsWith(".GlobalUsings.g.cs", StringComparison.OrdinalIgnoreCase);
    }

    public static IReadOnlyCollection<string> GetDocumentedModuleIds()
    {
        var docsRoot = Combine("docs", "modules");
        if (!Directory.Exists(docsRoot))
        {
            return Array.Empty<string>();
        }

        return Directory.EnumerateFiles(docsRoot, "*.md", SearchOption.AllDirectories)
            .Select(System.IO.Path.GetFileNameWithoutExtension)
            .Select(name => DocumentationModulePattern.Match(name ?? string.Empty))
            .Where(match => match.Success)
            .Select(match => match.Groups[1].Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static IReadOnlyCollection<string> GetImplementedModuleIds(params string[] relativeRoots)
    {
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var relativeRoot in relativeRoots)
        {
            var absoluteRoot = Combine(relativeRoot);
            if (!Directory.Exists(absoluteRoot))
            {
                continue;
            }

            foreach (var directory in Directory.EnumerateDirectories(absoluteRoot, "*", SearchOption.AllDirectories))
            {
                var match = HyphenatedModulePattern.Match(System.IO.Path.GetFileName(directory));
                if (match.Success)
                {
                    ids.Add(match.Groups[1].Value);
                }
            }
        }

        return ids.OrderBy(id => id, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            if (File.Exists(System.IO.Path.Combine(current.FullName, "SWFC.slnx")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the repository root containing SWFC.slnx.");
    }

    private static string NormalizePath(string path)
    {
        return path.Replace(System.IO.Path.AltDirectorySeparatorChar, System.IO.Path.DirectorySeparatorChar);
    }
}
