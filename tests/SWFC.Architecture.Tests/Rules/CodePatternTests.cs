using SWFC.Architecture.Tests.Support;

namespace SWFC.Architecture.Tests.Rules;

public sealed class CodePatternTests
{
    [Fact]
    public void Handwritten_Source_Files_Should_Stay_Within_Documented_Size_Guidelines()
    {
        var domainViolations = RepositoryRoot.EnumerateFiles("src\\SWFC.Domain", "*.cs", includeGeneratedFiles: false)
            .Select(GetLineCount)
            .Where(result => result.LineCount > 300)
            .Select(result => $"{result.RelativePath} ({result.LineCount} lines)");

        var applicationViolations = RepositoryRoot.EnumerateFiles("src\\SWFC.Application", "*.cs", includeGeneratedFiles: false)
            .Select(GetLineCount)
            .Where(result => result.LineCount > 250)
            .Select(result => $"{result.RelativePath} ({result.LineCount} lines)");

        var razorViolations = RepositoryRoot.EnumerateFiles("src\\SWFC.Web", "*.razor", includeGeneratedFiles: false)
            .Select(GetLineCount)
            .Where(result => result.LineCount > 400)
            .Select(result => $"{result.RelativePath} ({result.LineCount} lines)");

        var violations = domainViolations
            .Concat(applicationViolations)
            .Concat(razorViolations)
            .ToArray();

        Assert.True(
            violations.Length == 0,
            $"Files exceeding documented size limits: {string.Join(", ", violations)}");
    }

    [Fact]
    public void Source_Code_Should_Not_Use_Local_Time_Apis_For_Documented_Utc_Rules()
    {
        var offendingLines = RepositoryRoot.EnumerateFiles("src", "*.cs", includeGeneratedFiles: false)
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, index) => new
                {
                    RelativePath = RepositoryRoot.ToRelativePath(path),
                    LineNumber = index + 1,
                    Line = line
                }))
            .Where(entry => entry.Line.Contains("DateTime.Now", StringComparison.Ordinal)
                || entry.Line.Contains("DateTimeOffset.Now", StringComparison.Ordinal))
            .Select(entry => $"{entry.RelativePath}:{entry.LineNumber}")
            .ToArray();

        Assert.True(
            offendingLines.Length == 0,
            $"Local time APIs violate the UTC rule set: {string.Join(", ", offendingLines)}");
    }

    private static (string RelativePath, int LineCount) GetLineCount(string path)
    {
        var lineCount = File.ReadLines(path).Count();
        return (RepositoryRoot.ToRelativePath(path), lineCount);
    }
}
