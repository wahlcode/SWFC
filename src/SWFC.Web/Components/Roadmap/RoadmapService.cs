using System.Text.Json;

namespace SWFC.Web.Components.Roadmap;

public sealed class RoadmapService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IWebHostEnvironment _environment;

    public RoadmapService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<IReadOnlyList<RoadmapVersion>> GetVersionsAsync(
        CancellationToken cancellationToken = default)
    {
        var filePath = GetRoadmapFilePath();
        if (!File.Exists(filePath))
        {
            return Array.Empty<RoadmapVersion>();
        }

        await using var stream = File.OpenRead(filePath);
        var file = await JsonSerializer.DeserializeAsync<RoadmapFileDto>(stream, JsonOptions, cancellationToken);
        var versions = file?.Versions ?? new List<RoadmapVersion>();

        foreach (var version in versions)
        {
            Normalize(version);
        }

        return versions;
    }

    public async Task<RoadmapVersion?> GetCurrentVersionAsync(CancellationToken cancellationToken = default)
    {
        var versions = await GetVersionsAsync(cancellationToken);
        return versions.FirstOrDefault(x => x.Status == RoadmapStatuses.InProgress)
            ?? versions.LastOrDefault(x => x.Status == RoadmapStatuses.Done)
            ?? versions.FirstOrDefault();
    }

    public async Task<RoadmapVersion?> GetNextVersionAsync(CancellationToken cancellationToken = default)
    {
        var versions = await GetVersionsAsync(cancellationToken);
        var current = versions.FirstOrDefault(x => x.Status == RoadmapStatuses.InProgress);
        if (current is null)
        {
            return versions.FirstOrDefault(x => x.Status == RoadmapStatuses.Planned);
        }

        var currentIndex = versions.ToList().IndexOf(current);
        return versions
            .Skip(currentIndex + 1)
            .FirstOrDefault(x => x.Status == RoadmapStatuses.Planned || x.Status == RoadmapStatuses.InProgress);
    }

    public async Task<RoadmapVersion?> GetVersionAsync(
        string versionNumber,
        CancellationToken cancellationToken = default)
    {
        var versions = await GetVersionsAsync(cancellationToken);
        return versions.FirstOrDefault(x =>
            string.Equals(x.Version, versionNumber, StringComparison.OrdinalIgnoreCase));
    }

    private string GetRoadmapFilePath()
    {
        var contentRoot = new DirectoryInfo(_environment.ContentRootPath);

        for (var current = contentRoot; current is not null; current = current.Parent)
        {
            var candidate = Path.Combine(current.FullName, "docs", "M600-Planning", "M601-Roadmap", "roadmap.json");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            if (File.Exists(Path.Combine(current.FullName, "SWFC.slnx")) ||
                Directory.Exists(Path.Combine(current.FullName, ".git")))
            {
                return candidate;
            }
        }

        return Path.Combine(_environment.ContentRootPath, "docs", "M600-Planning", "M601-Roadmap", "roadmap.json");
    }

    private static void Normalize(RoadmapVersion version)
    {
        version.Version = (version.Version ?? string.Empty).Trim();
        version.Name = (version.Name ?? string.Empty).Trim();
        version.AudienceTitle = (version.AudienceTitle ?? string.Empty).Trim();
        version.Goal = (version.Goal ?? string.Empty).Trim();
        version.UserDescription = (version.UserDescription ?? string.Empty).Trim();
        version.DeveloperDescription = (version.DeveloperDescription ?? string.Empty).Trim();
        version.Status = NormalizeStatus(version.Status);
        version.PrimaryModules = NormalizeList(version.PrimaryModules);
        version.RequiredModules = NormalizeList(version.RequiredModules);
        version.Result = (version.Result ?? string.Empty).Trim();

        foreach (var milestone in version.Milestones)
        {
            milestone.Title = (milestone.Title ?? string.Empty).Trim();
            milestone.Description = (milestone.Description ?? string.Empty).Trim();
            milestone.Status = NormalizeStatus(milestone.Status);
        }
    }

    private static List<string> NormalizeList(IEnumerable<string> values)
    {
        return values
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string NormalizeStatus(string? status)
    {
        var normalized = (status ?? string.Empty).Trim().Replace(" ", string.Empty).ToLowerInvariant();

        return normalized switch
        {
            "done" => RoadmapStatuses.Done,
            "inprogress" => RoadmapStatuses.InProgress,
            _ => RoadmapStatuses.Planned
        };
    }
}
