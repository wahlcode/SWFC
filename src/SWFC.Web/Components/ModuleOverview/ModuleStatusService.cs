using System.Text.Json;
using System.Text.Json.Serialization;

namespace SWFC.Web.Components.ModuleOverview;

public sealed class ModuleOverviewFileDto
{
    public List<ModuleOverviewGroupDto> Groups { get; set; } = new();
}

public sealed class ModuleOverviewGroupDto
{
    public string GroupCode { get; set; } = string.Empty;
    public string GroupTitle { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<ModuleOverviewModuleDto> Modules { get; set; } = new();
}

public sealed class ModuleOverviewModuleDto
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public string Status { get; set; } = ModuleStatuses.Open;
    public string Level { get; set; } = ModuleLevels.Core;
    public List<ModuleOverviewWorkItemDto> WorkItems { get; set; } = new();

    [JsonIgnore]
    public string AuditStatus { get; set; } = ModuleAuditStatuses.NotChecked;

    [JsonIgnore]
    public int? AuditScorePercent { get; set; }

    [JsonIgnore]
    public string? AuditSource { get; set; }

    [JsonIgnore]
    public DateTimeOffset? LastAuditUtc { get; set; }

    [JsonIgnore]
    public string CodeStatus { get; set; } = ModuleAuditPartStatuses.NotChecked;

    [JsonIgnore]
    public string MdStatus { get; set; } = ModuleAuditPartStatuses.NotChecked;

    [JsonIgnore]
    public string TestStatus { get; set; } = ModuleAuditPartStatuses.NotChecked;

    [JsonIgnore]
    public List<string> AuditFlags { get; set; } = new();

    [JsonIgnore]
    public List<ModuleAuditIssue> AuditIssues { get; set; } = new();

    public int DoneCount => WorkItems.Count(x => NormalizeWorkItemStatus(x.Status) == ModuleWorkItemStatuses.Done);
    public int InProgressCount => WorkItems.Count(x => NormalizeWorkItemStatus(x.Status) == ModuleWorkItemStatuses.InProgress);
    public int OpenCount => WorkItems.Count(x => NormalizeWorkItemStatus(x.Status) == ModuleWorkItemStatuses.Open);
    public int TotalCount => WorkItems.Count;

    public int CoreDoneCount => WorkItems.Count(x =>
        NormalizeLevel(x.Level) == ModuleLevels.Core &&
        NormalizeWorkItemStatus(x.Status) == ModuleWorkItemStatuses.Done);

    public int CoreTotalCount => WorkItems.Count(x => NormalizeLevel(x.Level) == ModuleLevels.Core);

    public int OptionalCoreDoneCount => WorkItems.Count(x =>
        NormalizeLevel(x.Level) == ModuleLevels.OptionalCore &&
        NormalizeWorkItemStatus(x.Status) == ModuleWorkItemStatuses.Done);

    public int OptionalCoreTotalCount => WorkItems.Count(x => NormalizeLevel(x.Level) == ModuleLevels.OptionalCore);

    public int ExtensionDoneCount => WorkItems.Count(x =>
        NormalizeLevel(x.Level) == ModuleLevels.Extension &&
        NormalizeWorkItemStatus(x.Status) == ModuleWorkItemStatuses.Done);

    public int ExtensionTotalCount => WorkItems.Count(x => NormalizeLevel(x.Level) == ModuleLevels.Extension);

    public int ProgressPercent =>
        TotalCount == 0
            ? 0
            : (int)Math.Round((decimal)DoneCount / TotalCount * 100m, 0, MidpointRounding.AwayFromZero);

    internal static string NormalizeLevel(string? value)
    {
        var normalized = (value ?? string.Empty).Trim().ToLowerInvariant();

        return normalized switch
        {
            "optionalcore" => ModuleLevels.OptionalCore,
            "optional" => ModuleLevels.OptionalCore,
            "extension" => ModuleLevels.Extension,
            _ => ModuleLevels.Core
        };
    }

    internal static string NormalizeWorkItemStatus(string? value)
    {
        var normalized = (value ?? string.Empty).Trim().ToLowerInvariant();

        return normalized switch
        {
            "done" => ModuleWorkItemStatuses.Done,
            "inprogress" => ModuleWorkItemStatuses.InProgress,
            "in progress" => ModuleWorkItemStatuses.InProgress,
            _ => ModuleWorkItemStatuses.Open
        };
    }
}

public sealed class ModuleOverviewWorkItemDto
{
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = ModuleWorkItemStatuses.Open;
    public string Level { get; set; } = ModuleLevels.Core;
}

public sealed class ModuleOverviewSummaryDto
{
    public int TotalModules { get; set; }
    public int OpenCount { get; set; }
    public int InProgressCount { get; set; }
    public int CoreCompleteCount { get; set; }
    public int ExtendedCompleteCount { get; set; }
    public int FullCompleteCount { get; set; }
    public int CoreCount { get; set; }
    public int OptionalCoreCount { get; set; }
    public int ExtensionCount { get; set; }
    public int TotalWorkItems { get; set; }
}

public sealed class ModuleOverviewDto
{
    public List<ModuleOverviewGroupDto> Groups { get; set; } = new();
    public ModuleOverviewSummaryDto Summary { get; set; } = new();
}

public sealed class UpdateWorkItemStatusRequest
{
    public string GroupCode { get; set; } = string.Empty;
    public string ModuleCode { get; set; } = string.Empty;
    public string WorkItemTitle { get; set; } = string.Empty;
    public string Status { get; set; } = ModuleWorkItemStatuses.Open;
}

public sealed class ModuleStatusService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private readonly IWebHostEnvironment _environment;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ModuleAuditService _moduleAuditService;
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    public ModuleStatusService(
        IWebHostEnvironment environment,
        IHttpContextAccessor httpContextAccessor,
        ModuleAuditService moduleAuditService)
    {
        _environment = environment;
        _httpContextAccessor = httpContextAccessor;
        _moduleAuditService = moduleAuditService;
    }

    public async Task<ModuleOverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        await _writeLock.WaitAsync(cancellationToken);
        try
        {
            var file = await LoadFileAsync(cancellationToken);
            var groups = file.Groups ?? new List<ModuleOverviewGroupDto>();

            NormalizeAndCompute(groups);
            var auditResults = await _moduleAuditService.GenerateAuditResultsAsync(groups, cancellationToken);
            ApplyAuditResults(groups, auditResults);

            return BuildOverview(groups);
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public async Task<bool> UpdateWorkItemStatusAsync(UpdateWorkItemStatusRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var username = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
        if (!string.Equals(username, "developer", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var normalizedStatus = ModuleOverviewModuleDto.NormalizeWorkItemStatus(request.Status);

        await _writeLock.WaitAsync(cancellationToken);
        try
        {
            var file = await LoadFileAsync(cancellationToken);

            var group = file.Groups.FirstOrDefault(x =>
                string.Equals(x.GroupCode?.Trim(), request.GroupCode?.Trim(), StringComparison.OrdinalIgnoreCase));

            if (group is null)
            {
                return false;
            }

            var module = group.Modules.FirstOrDefault(x =>
                string.Equals(x.Code?.Trim(), request.ModuleCode?.Trim(), StringComparison.OrdinalIgnoreCase));

            if (module is null)
            {
                return false;
            }

            var workItem = module.WorkItems.FirstOrDefault(x =>
                string.Equals(x.Title?.Trim(), request.WorkItemTitle?.Trim(), StringComparison.OrdinalIgnoreCase));

            if (workItem is null)
            {
                return false;
            }

            workItem.Status = normalizedStatus;

            await SaveFileAsync(file, cancellationToken);

            return true;
        }
        finally
        {
            _writeLock.Release();
        }
    }

    private async Task<ModuleOverviewFileDto> LoadFileAsync(CancellationToken cancellationToken)
    {
        var filePath = GetModulesFilePath();

        if (!File.Exists(filePath))
        {
            return new ModuleOverviewFileDto();
        }

        await using var stream = File.OpenRead(filePath);
        var file = await JsonSerializer.DeserializeAsync<ModuleOverviewFileDto>(stream, JsonOptions, cancellationToken);

        return file ?? new ModuleOverviewFileDto();
    }

    private async Task SaveFileAsync(ModuleOverviewFileDto file, CancellationToken cancellationToken)
    {
        var filePath = GetModulesFilePath();
        await using var writeStream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(writeStream, file, JsonOptions, cancellationToken);
    }

    private string GetModulesFilePath()
    {
        return Path.Combine(_environment.WebRootPath, "data", "modules.json");
    }

    private static void NormalizeAndCompute(IEnumerable<ModuleOverviewGroupDto> groups)
    {
        foreach (var group in groups)
        {
            group.GroupCode = (group.GroupCode ?? string.Empty).Trim();
            group.GroupTitle = (group.GroupTitle ?? string.Empty).Trim();
            group.Description = (group.Description ?? string.Empty).Trim();

            foreach (var module in group.Modules)
            {
                module.Code = (module.Code ?? string.Empty).Trim();
                module.Title = (module.Title ?? string.Empty).Trim();
                module.Description = (module.Description ?? string.Empty).Trim();
                module.Level = ModuleOverviewModuleDto.NormalizeLevel(module.Level);

                foreach (var workItem in module.WorkItems)
                {
                    workItem.Title = (workItem.Title ?? string.Empty).Trim();
                    workItem.Level = ModuleOverviewModuleDto.NormalizeLevel(workItem.Level);
                    workItem.Status = ModuleOverviewModuleDto.NormalizeWorkItemStatus(workItem.Status);
                }

                module.Status = ComputeModuleStatus(module.WorkItems);
            }
        }
    }

    private static void ApplyAuditResults(
        IEnumerable<ModuleOverviewGroupDto> groups,
        IReadOnlyDictionary<string, ModuleAuditResult> auditResults)
    {
        foreach (var module in groups.SelectMany(x => x.Modules))
        {
            if (!auditResults.TryGetValue(module.Code, out var audit))
            {
                module.AuditStatus = ModuleAuditStatuses.NotChecked;
                module.AuditScorePercent = null;
                module.AuditSource = null;
                module.LastAuditUtc = null;
                module.CodeStatus = ModuleAuditPartStatuses.NotChecked;
                module.MdStatus = ModuleAuditPartStatuses.NotChecked;
                module.TestStatus = ModuleAuditPartStatuses.NotChecked;
                module.AuditFlags = new List<string>();
                module.AuditIssues = new List<ModuleAuditIssue>();
                continue;
            }

            module.AuditStatus = audit.Status;
            module.AuditScorePercent = audit.ScorePercent;
            module.AuditSource = audit.Source;
            module.LastAuditUtc = audit.LastAuditUtc;
            module.CodeStatus = audit.CodeStatus;
            module.MdStatus = audit.MdStatus;
            module.TestStatus = audit.TestStatus;
            module.AuditFlags = audit.Flags;
            module.AuditIssues = audit.Issues;
        }
    }

    private static ModuleOverviewDto BuildOverview(List<ModuleOverviewGroupDto> groups)
    {
        var modules = groups.SelectMany(x => x.Modules).ToList();
        var workItems = modules.SelectMany(x => x.WorkItems).ToList();

        return new ModuleOverviewDto
        {
            Groups = groups,
            Summary = new ModuleOverviewSummaryDto
            {
                TotalModules = modules.Count,
                OpenCount = modules.Count(x => x.Status == ModuleStatuses.Open),
                InProgressCount = modules.Count(x => x.Status == ModuleStatuses.InProgress),
                CoreCompleteCount = modules.Count(x => x.Status == ModuleStatuses.CoreComplete),
                ExtendedCompleteCount = modules.Count(x => x.Status == ModuleStatuses.ExtendedComplete),
                FullCompleteCount = modules.Count(x => x.Status == ModuleStatuses.FullComplete),
                CoreCount = modules.Count(x => ModuleOverviewModuleDto.NormalizeLevel(x.Level) == ModuleLevels.Core),
                OptionalCoreCount = modules.Count(x => ModuleOverviewModuleDto.NormalizeLevel(x.Level) == ModuleLevels.OptionalCore),
                ExtensionCount = modules.Count(x => ModuleOverviewModuleDto.NormalizeLevel(x.Level) == ModuleLevels.Extension),
                TotalWorkItems = workItems.Count
            }
        };
    }

    private static string ComputeModuleStatus(IReadOnlyCollection<ModuleOverviewWorkItemDto> workItems)
    {
        if (workItems.Count == 0)
        {
            return ModuleStatuses.Open;
        }

        var normalizedItems = workItems
            .Select(x => new
            {
                Status = ModuleOverviewModuleDto.NormalizeWorkItemStatus(x.Status),
                Level = ModuleOverviewModuleDto.NormalizeLevel(x.Level)
            })
            .ToList();

        var anyNotOpen = normalizedItems.Any(x => x.Status != ModuleWorkItemStatuses.Open);
        var allDone = normalizedItems.All(x => x.Status == ModuleWorkItemStatuses.Done);

        var coreItems = normalizedItems
            .Where(x => x.Level == ModuleLevels.Core)
            .ToList();

        var coreAndOptionalItems = normalizedItems
            .Where(x => x.Level == ModuleLevels.Core || x.Level == ModuleLevels.OptionalCore)
            .ToList();

        var allCoreDone = coreItems.Count > 0 && coreItems.All(x => x.Status == ModuleWorkItemStatuses.Done);
        var allCoreAndOptionalDone = coreAndOptionalItems.Count > 0 && coreAndOptionalItems.All(x => x.Status == ModuleWorkItemStatuses.Done);

        if (allDone)
        {
            return ModuleStatuses.FullComplete;
        }

        if (allCoreAndOptionalDone)
        {
            return ModuleStatuses.ExtendedComplete;
        }

        if (allCoreDone)
        {
            return ModuleStatuses.CoreComplete;
        }

        if (anyNotOpen)
        {
            return ModuleStatuses.InProgress;
        }

        return ModuleStatuses.Open;
    }
}

public static class ModuleLevels
{
    public const string Core = "Core";
    public const string OptionalCore = "OptionalCore";
    public const string Extension = "Extension";
}

public static class ModuleWorkItemStatuses
{
    public const string Open = "Open";
    public const string InProgress = "InProgress";
    public const string Done = "Done";
}

public static class ModuleStatuses
{
    public const string Open = "Open";
    public const string InProgress = "In Progress";
    public const string CoreComplete = "Core Complete";
    public const string ExtendedComplete = "Extended Complete";
    public const string FullComplete = "Full Complete";
}
