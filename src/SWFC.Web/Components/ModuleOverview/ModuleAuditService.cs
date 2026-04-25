using System.Text.Json;

namespace SWFC.Web.Components.ModuleOverview;

public sealed class ModuleAuditService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private readonly IWebHostEnvironment _environment;

    public ModuleAuditService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<IReadOnlyDictionary<string, ModuleAuditResult>> LoadAuditResultsAsync(
        CancellationToken cancellationToken = default)
    {
        var generatedPath = GetGeneratedStatusPath();

        if (!Directory.Exists(generatedPath))
        {
            return new Dictionary<string, ModuleAuditResult>(StringComparer.OrdinalIgnoreCase);
        }

        var results = new Dictionary<string, ModuleAuditResult>(StringComparer.OrdinalIgnoreCase);

        foreach (var filePath in Directory.EnumerateFiles(generatedPath, "*.json"))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = await LoadAuditResultAsync(filePath, cancellationToken);
            if (result is null || string.IsNullOrWhiteSpace(result.ModuleCode))
            {
                continue;
            }

            Normalize(result);
            results[result.ModuleCode] = result;
        }

        return results;
    }

    public async Task<IReadOnlyDictionary<string, ModuleAuditResult>> GenerateAuditResultsAsync(
        IEnumerable<ModuleOverviewGroupDto> groups,
        CancellationToken cancellationToken = default)
    {
        var generatedPath = GetGeneratedStatusPath();
        Directory.CreateDirectory(generatedPath);

        var repositoryRoot = GetRepositoryRoot();
        var sourceFiles = GetCandidateFiles(Path.Combine(repositoryRoot, "src"), "*.cs");
        var documentationFiles = GetCandidateFiles(Path.Combine(repositoryRoot, "docs", "modules"), "*.md");
        var testFiles = GetCandidateFiles(Path.Combine(repositoryRoot, "tests"), "*.cs");
        var results = new Dictionary<string, ModuleAuditResult>(StringComparer.OrdinalIgnoreCase);

        foreach (var module in groups.SelectMany(x => x.Modules))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = CreateAuditResult(module, sourceFiles, documentationFiles, testFiles);
            results[result.ModuleCode] = result;
            var filePath = Path.Combine(generatedPath, $"{result.ModuleCode}.json");

            await using var writeStream = File.Create(filePath);
            await JsonSerializer.SerializeAsync(writeStream, result, JsonOptions, cancellationToken);
        }

        return results;
    }

    private async Task<ModuleAuditResult?> LoadAuditResultAsync(
        string filePath,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = File.OpenRead(filePath);
            return await JsonSerializer.DeserializeAsync<ModuleAuditResult>(stream, JsonOptions, cancellationToken);
        }
        catch (JsonException)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }
    }

    private string GetGeneratedStatusPath()
    {
        var repositoryRoot = GetRepositoryRoot();
        return Path.Combine(repositoryRoot, "status", "generated");
    }

    private string GetRepositoryRoot()
    {
        var contentRoot = new DirectoryInfo(_environment.ContentRootPath);

        for (var current = contentRoot; current is not null; current = current.Parent)
        {
            if (File.Exists(Path.Combine(current.FullName, "SWFC.slnx")) ||
                Directory.Exists(Path.Combine(current.FullName, ".git")))
            {
                return current.FullName;
            }
        }

        return _environment.ContentRootPath;
    }

    private static List<string> GetCandidateFiles(string path, string pattern)
    {
        if (!Directory.Exists(path))
        {
            return new List<string>();
        }

        return Directory.EnumerateFiles(path, pattern, SearchOption.AllDirectories)
            .Where(x =>
                !x.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase) &&
                !x.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private static ModuleAuditResult CreateAuditResult(
        ModuleOverviewModuleDto module,
        IReadOnlyCollection<string> sourceFiles,
        IReadOnlyCollection<string> documentationFiles,
        IReadOnlyCollection<string> testFiles)
    {
        var moduleCode = module.Code.Trim();
        var moduleSourceFiles = sourceFiles
            .Where(x => PathContainsModuleCode(x, moduleCode))
            .ToList();
        var moduleDocumentationFiles = documentationFiles
            .Where(x => PathContainsModuleCode(x, moduleCode))
            .ToList();
        var moduleTestFiles = testFiles
            .Where(x => PathContainsModuleCode(x, moduleCode) || FileContains(x, moduleCode))
            .ToList();

        var codeFound = moduleSourceFiles.Count > 0;
        var testsFound = moduleTestFiles.Count > 0;
        var hasProfile = ModuleAuditProfiles.TryGet(moduleCode, out var profile);
        var workItemResults = module.WorkItems
            .Where(x => !IsGeneratedAuditWorkItem(x.Title))
            .Select(x => hasProfile && TryGetWorkItemProfile(profile!, x.Title, out var workItemProfile)
                ? CreateProfiledWorkItemResult(
                    x,
                    workItemProfile,
                    sourceFiles,
                    moduleDocumentationFiles,
                    moduleTestFiles)
                : CreateWorkItemResult(x, moduleSourceFiles, moduleDocumentationFiles, moduleTestFiles))
            .ToList();
        var documentationStatus = GetDocumentationStatus(moduleDocumentationFiles, workItemResults);
        var codeStatus = GetAggregatePartStatus(workItemResults, "Code");
        var testStatus = GetAggregatePartStatus(workItemResults, "Tests");
        var score = CalculateScore(workItemResults);

        var flags = new List<string>();
        var issues = new List<ModuleAuditIssue>();

        if (!codeFound || codeStatus == ModuleAuditPartStatuses.Missing)
        {
            flags.Add(ModuleAuditFlags.MissingCode);
            issues.Add(new ModuleAuditIssue
            {
                Severity = "Warning",
                Message = $"Keine Implementierung für {moduleCode} gefunden."
            });
        }

        if (documentationStatus == ModuleAuditPartStatuses.Incomplete)
        {
            flags.Add(ModuleAuditFlags.MdIncomplete);
            issues.Add(new ModuleAuditIssue
            {
                Severity = "Info",
                Message = "MD-Dokumentation ist noch nicht vollständig."
            });
        }
        else if (documentationStatus == ModuleAuditPartStatuses.Missing)
        {
            flags.Add("missing_md");
            issues.Add(new ModuleAuditIssue
            {
                Severity = "Warning",
                Message = $"Keine MD-Dokumentation für {moduleCode} gefunden."
            });
        }

        if (!testsFound || testStatus == ModuleAuditPartStatuses.Missing)
        {
            flags.Add(ModuleAuditFlags.MissingTests);
            issues.Add(new ModuleAuditIssue
            {
                Severity = "Info",
                Message = $"Keine Tests mit Bezug zu {moduleCode} gefunden."
            });
        }

        var openOrPartialWorkItemCount = workItemResults.Count(x => x.Status != ModuleWorkItemStatuses.Done);
        if (openOrPartialWorkItemCount > 0)
        {
            flags.Add("workitems_not_verified");
            issues.Add(new ModuleAuditIssue
            {
                Severity = "Info",
                Message = $"{openOrPartialWorkItemCount} Arbeitspunkte sind nicht vollständig durch Dokumentation, Code und Tests belegt."
            });
        }

        return new ModuleAuditResult
        {
            ModuleCode = moduleCode,
            ScorePercent = score,
            Status = score switch
            {
                >= 80 => ModuleAuditStatuses.Verified,
                > 0 => ModuleAuditStatuses.PartiallyVerified,
                _ => ModuleAuditStatuses.NotChecked
            },
            Source = ModuleAuditSources.Generated,
            LastAuditUtc = DateTimeOffset.UtcNow,
            CodeStatus = codeStatus,
            MdStatus = documentationStatus,
            TestStatus = testStatus,
            Flags = flags,
            Issues = issues,
            WorkItems = workItemResults
        };
    }

    private static bool PathContainsModuleCode(string path, string moduleCode)
    {
        var fileName = Path.GetFileNameWithoutExtension(path);
        if (IsModuleCodePrefix(fileName, moduleCode))
        {
            return true;
        }

        return path
            .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Any(segment => IsModuleCodePrefix(segment, moduleCode));
    }

    private static bool IsModuleCodePrefix(string value, string moduleCode)
    {
        if (string.Equals(value, moduleCode, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!value.StartsWith(moduleCode, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return value.Length > moduleCode.Length &&
            !char.IsLetterOrDigit(value[moduleCode.Length]);
    }

    private static string GetDocumentationStatus(
        IReadOnlyCollection<string> moduleDocumentationFiles,
        IReadOnlyCollection<ModuleAuditWorkItemResult> workItemResults)
    {
        if (moduleDocumentationFiles.Count == 0)
        {
            return ModuleAuditPartStatuses.Missing;
        }

        if (workItemResults.Count == 0)
        {
            return ModuleAuditPartStatuses.Incomplete;
        }

        var documentedWorkItems = workItemResults.Count(x => EvidenceContains(x.Evidence, "MD"));
        if (documentedWorkItems == 0)
        {
            return ModuleAuditPartStatuses.Missing;
        }

        return documentedWorkItems == workItemResults.Count
            ? ModuleAuditPartStatuses.Complete
            : ModuleAuditPartStatuses.Incomplete;
    }

    private static ModuleAuditWorkItemResult CreateWorkItemResult(
        ModuleOverviewWorkItemDto workItem,
        IReadOnlyCollection<string> moduleSourceFiles,
        IReadOnlyCollection<string> moduleDocumentationFiles,
        IReadOnlyCollection<string> moduleTestFiles)
    {
        var sourceMatch = HasWorkItemEvidence(workItem.Title, moduleSourceFiles, includeContent: true);
        var documentationMatch = HasWorkItemEvidence(workItem.Title, moduleDocumentationFiles, includeContent: true);
        var testMatch = HasWorkItemEvidence(workItem.Title, moduleTestFiles, includeContent: true);

        return CreateWorkItemResult(workItem.Title, sourceMatch, documentationMatch, testMatch);
    }

    private static ModuleAuditWorkItemResult CreateProfiledWorkItemResult(
        ModuleOverviewWorkItemDto workItem,
        ModuleAuditEvidenceProfile profile,
        IReadOnlyCollection<string> moduleSourceFiles,
        IReadOnlyCollection<string> moduleDocumentationFiles,
        IReadOnlyCollection<string> moduleTestFiles)
    {
        var sourceMatch = HasExplicitEvidence(moduleSourceFiles, profile.CodeTerms);
        var documentationMatch = HasExplicitEvidence(moduleDocumentationFiles, profile.DocumentationTerms);
        var testMatch = HasExplicitEvidence(moduleTestFiles, profile.TestTerms);

        return CreateWorkItemResult(workItem.Title, sourceMatch, documentationMatch, testMatch);
    }

    private static ModuleAuditWorkItemResult CreateWorkItemResult(
        string title,
        bool sourceMatch,
        bool documentationMatch,
        bool testMatch)
    {
        var status = (documentationMatch, sourceMatch, testMatch) switch
        {
            (true, true, true) => ModuleWorkItemStatuses.Done,
            (true, true, false) => ModuleWorkItemStatuses.InProgress,
            (true, false, true) => ModuleWorkItemStatuses.InProgress,
            (true, false, false) => ModuleWorkItemStatuses.Open,
            (false, true, _) => ModuleWorkItemStatuses.InProgress,
            (false, false, true) => ModuleWorkItemStatuses.InProgress,
            _ => ModuleWorkItemStatuses.Open
        };

        var evidenceParts = new List<string>();
        if (sourceMatch)
        {
            evidenceParts.Add("Code");
        }

        if (documentationMatch)
        {
            evidenceParts.Add("MD");
        }

        if (testMatch)
        {
            evidenceParts.Add("Tests");
        }

        return new ModuleAuditWorkItemResult
        {
            Title = title,
            Status = status,
            Evidence = evidenceParts.Count == 0 ? "Keine Evidenz" : string.Join(", ", evidenceParts)
        };
    }

    private static string GetAggregatePartStatus(
        IReadOnlyCollection<ModuleAuditWorkItemResult> workItemResults,
        string evidenceName)
    {
        if (workItemResults.Count == 0)
        {
            return ModuleAuditPartStatuses.Missing;
        }

        var matchingCount = workItemResults.Count(x => EvidenceContains(x.Evidence, evidenceName));
        if (matchingCount == 0)
        {
            return ModuleAuditPartStatuses.Missing;
        }

        return matchingCount == workItemResults.Count
            ? ModuleAuditPartStatuses.Complete
            : ModuleAuditPartStatuses.Incomplete;
    }

    private static int CalculateScore(IReadOnlyCollection<ModuleAuditWorkItemResult> workItemResults)
    {
        if (workItemResults.Count == 0)
        {
            return 0;
        }

        var points = workItemResults.Sum(x => x.Status switch
        {
            ModuleWorkItemStatuses.Done => 100,
            ModuleWorkItemStatuses.InProgress => 50,
            _ => 0
        });

        return (int)Math.Round((decimal)points / workItemResults.Count, 0, MidpointRounding.AwayFromZero);
    }

    private static bool EvidenceContains(string evidence, string value)
    {
        return evidence.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Any(x => string.Equals(x, value, StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasWorkItemEvidence(
        string title,
        IReadOnlyCollection<string> files,
        bool includeContent)
    {
        if (files.Count == 0)
        {
            return false;
        }

        var terms = BuildSearchTerms(title);
        if (terms.Count == 0)
        {
            return false;
        }

        return files.Any(filePath =>
        {
            var searchablePath = NormalizeSearchText(Path.GetFileNameWithoutExtension(filePath));
            var pathMatch = terms.Any(term => searchablePath.Contains(term, StringComparison.Ordinal));
            if (pathMatch || !includeContent)
            {
                return pathMatch;
            }

            var searchableContent = NormalizeSearchText(File.ReadAllText(filePath));
            return terms.Any(term => searchableContent.Contains(term, StringComparison.Ordinal));
        });
    }

    private static bool TryGetWorkItemProfile(
        ModuleAuditProfile profile,
        string title,
        out ModuleAuditEvidenceProfile workItemProfile)
    {
        var normalizedTitle = NormalizeSearchText(title);
        KeyValuePair<string, ModuleAuditEvidenceProfile>? bestMatch = null;
        var bestScore = 0;

        foreach (var entry in profile.WorkItems)
        {
            var normalizedKey = NormalizeSearchText(entry.Key);

            if (string.Equals(normalizedKey, normalizedTitle, StringComparison.Ordinal))
            {
                workItemProfile = entry.Value;
                return true;
            }

            var score = GetProfileMatchScore(normalizedTitle, normalizedKey);
            if (score > bestScore)
            {
                bestScore = score;
                bestMatch = entry;
            }
        }

        if (bestMatch.HasValue && bestScore > 0)
        {
            workItemProfile = bestMatch.Value.Value;
            return true;
        }

        workItemProfile = default!;
        return false;
    }

    private static int GetProfileMatchScore(string normalizedTitle, string normalizedKey)
    {
        var titleTerms = normalizedTitle
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => x.Length >= 4)
            .ToHashSet(StringComparer.Ordinal);
        var keyTerms = normalizedKey
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => x.Length >= 4)
            .ToArray();

        return keyTerms.Count(term => titleTerms.Contains(term));
    }

    private static bool HasExplicitEvidence(
        IReadOnlyCollection<string> files,
        IReadOnlyCollection<string> terms)
    {
        if (files.Count == 0 || terms.Count == 0)
        {
            return false;
        }

        var normalizedTerms = terms
            .Select(NormalizeSearchText)
            .Where(x => x.Length >= 3)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (normalizedTerms.Length == 0)
        {
            return false;
        }

        return files.Any(filePath =>
        {
            var searchablePath = NormalizeSearchText(Path.GetFileNameWithoutExtension(filePath));
            if (normalizedTerms.Any(term => searchablePath.Contains(term, StringComparison.Ordinal)))
            {
                return true;
            }

            var searchableContent = NormalizeSearchText(File.ReadAllText(filePath));
            return normalizedTerms.Any(term => searchableContent.Contains(term, StringComparison.Ordinal));
        });
    }

    private static List<string> BuildSearchTerms(string title)
    {
        var normalizedTitle = NormalizeSearchText(title);
        var terms = new List<string>();

        if (normalizedTitle.Length >= 4)
        {
            terms.Add(normalizedTitle);
        }

        terms.AddRange(normalizedTitle
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => x.Length >= 4)
            .Distinct(StringComparer.Ordinal));

        return terms;
    }

    private static string NormalizeSearchText(string value)
    {
        var characters = value
            .ToLowerInvariant()
            .Select(x => char.IsLetterOrDigit(x) ? x : ' ')
            .ToArray();

        return string.Join(
            ' ',
            new string(characters).Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }

    private static bool FileContains(string filePath, string value)
    {
        return File.ReadAllText(filePath).Contains(value, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsGeneratedAuditWorkItem(string? title)
    {
        return (title ?? string.Empty).Trim().StartsWith("Auto-Audit:", StringComparison.OrdinalIgnoreCase);
    }

    private static void Normalize(ModuleAuditResult result)
    {
        result.ModuleCode = (result.ModuleCode ?? string.Empty).Trim();
        result.Status = string.IsNullOrWhiteSpace(result.Status)
            ? ModuleAuditStatuses.NotChecked
            : result.Status.Trim();
        result.Source = string.IsNullOrWhiteSpace(result.Source)
            ? ModuleAuditSources.Generated
            : result.Source.Trim();
        result.CodeStatus = NormalizePartStatus(result.CodeStatus);
        result.MdStatus = NormalizePartStatus(result.MdStatus);
        result.TestStatus = NormalizePartStatus(result.TestStatus);
        result.ScorePercent = Math.Clamp(result.ScorePercent, 0, 100);
        result.Flags = result.Flags
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        result.Issues = result.Issues
            .Where(x => !string.IsNullOrWhiteSpace(x.Message))
            .ToList();
        result.WorkItems = result.WorkItems
            .Where(x => !string.IsNullOrWhiteSpace(x.Title))
            .ToList();
    }

    private static string NormalizePartStatus(string? status)
    {
        return string.IsNullOrWhiteSpace(status)
            ? ModuleAuditPartStatuses.NotChecked
            : status.Trim();
    }
}
