using System.Text.RegularExpressions;
using SWFC.Architecture.Tests.Support;

namespace SWFC.Architecture.Tests.Rules;

public sealed class NamingConventionTests
{
    private static readonly string[] ForbiddenGenericNames =
    {
        "Helper.cs",
        "Manager.cs",
        "Processor.cs",
        "Util.cs",
        "Common.cs",
        "Misc.cs",
        "Stuff.cs",
        "DataService.cs",
        "Temp.cs",
        "NewFile.cs"
    };

    private static readonly Regex ModuleDirectoryPattern = new(
        @"^M\d{3,4}-[A-Za-z0-9]+(?:-[A-Za-z0-9]+)*$",
        RegexOptions.Compiled);

    private static readonly Regex ModuleDirectoryCandidatePattern = new(
        @"^M\d{3,4}",
        RegexOptions.Compiled);

    [Fact]
    public void Application_Should_Not_Use_Technical_Slice_Folders()
    {
        var forbiddenFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Commands",
            "Queries",
            "Handlers",
            "Validators"
        };

        var directories = Directory.EnumerateDirectories(
                RepositoryRoot.Combine("src", "SWFC.Application"),
                "*",
                SearchOption.AllDirectories)
            .Where(path => forbiddenFolders.Contains(System.IO.Path.GetFileName(path)))
            .ToArray();

        Assert.True(
            directories.Length == 0,
            $"Technical slice folders violate the documented application structure: {string.Join(", ", directories)}");
    }

    [Fact]
    public void Source_Files_Should_Not_Use_Documented_Forbidden_Generic_Names()
    {
        var offendingFiles = Directory.EnumerateFiles(
                RepositoryRoot.Combine("src"),
                "*.*",
                SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{System.IO.Path.DirectorySeparatorChar}bin{System.IO.Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains($"{System.IO.Path.DirectorySeparatorChar}obj{System.IO.Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(path => ForbiddenGenericNames.Contains(System.IO.Path.GetFileName(path), StringComparer.OrdinalIgnoreCase))
            .ToArray();

        Assert.True(
            offendingFiles.Length == 0,
            $"Forbidden generic file names found: {string.Join(", ", offendingFiles)}");
    }

    [Fact]
    public void Module_Directories_Should_Follow_Documented_Naming_Format()
    {
        var roots = new[]
        {
            RepositoryRoot.Combine("src", "SWFC.Domain"),
            RepositoryRoot.Combine("src", "SWFC.Application")
        };

        var invalidDirectories = roots
            .Where(Directory.Exists)
            .SelectMany(root => Directory.EnumerateDirectories(root, "*", SearchOption.AllDirectories))
            .Select(path => new { path, name = System.IO.Path.GetFileName(path) })
            .Where(entry => ModuleDirectoryCandidatePattern.IsMatch(entry.name))
            .Where(entry => !ModuleDirectoryPattern.IsMatch(entry.name))
            .Select(entry => entry.path)
            .ToArray();

        Assert.True(
            invalidDirectories.Length == 0,
            $"Module directories must follow the Mxxx-Name pattern: {string.Join(", ", invalidDirectories)}");
    }
}
