using Microsoft.AspNetCore.Components.Authorization;
using SWFC.Web.Components.ModuleOverview;

namespace SWFC.Web.Pages.M600_Planning.ModuleOverview;

public partial class Modules
{
    private ModuleOverviewDto Overview = new();
    private ModuleOverviewState State = new();

    private bool CanEditWorkItems;
    private string? SavingWorkItemKey;
    private string? StatusMessage;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();

        CanEditWorkItems = string.Equals(
            authState.User.Identity?.Name,
            "developer",
            StringComparison.OrdinalIgnoreCase);

        await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        Overview = await ModuleStatusService.GetOverviewAsync();
        StateHasChanged();
    }

    private async Task OnWorkItemStatusChangedAsync(string groupCode, string moduleCode, string workItemTitle, string? newStatus)
    {
        if (!CanEditWorkItems)
        {
            StatusMessage = "Keine Berechtigung zum Ändern.";
            return;
        }

        var normalizedStatus = ModuleOverviewModuleDto.NormalizeWorkItemStatus(newStatus);
        var workItemKey = BuildWorkItemKey(groupCode, moduleCode, workItemTitle);

        SavingWorkItemKey = workItemKey;
        StatusMessage = null;

        try
        {
            var updated = await ModuleStatusService.UpdateWorkItemStatusAsync(
                new UpdateWorkItemStatusRequest
                {
                    GroupCode = groupCode,
                    ModuleCode = moduleCode,
                    WorkItemTitle = workItemTitle,
                    Status = normalizedStatus
                });

            if (updated)
            {
                await ReloadAsync();
                StatusMessage = $"Status gespeichert: {workItemTitle} -> {FormatWorkItemStatus(normalizedStatus)}";
            }
            else
            {
                StatusMessage = "Speichern fehlgeschlagen. Bitte Benutzername, Datei und Schreibzugriff prüfen.";
            }
        }
        finally
        {
            SavingWorkItemKey = null;
        }
    }

    private static string BuildWorkItemKey(string groupCode, string moduleCode, string workItemTitle)
    {
        return $"{groupCode}::{moduleCode}::{workItemTitle}";
    }

    private static int GetPercent(int count, int total)
    {
        if (total <= 0)
        {
            return 0;
        }

        return (int)Math.Round((decimal)count / total * 100m, 0, MidpointRounding.AwayFromZero);
    }

    private static string GetLevelLabel(string? level)
    {
        if (string.Equals(level, ModuleLevels.OptionalCore, StringComparison.OrdinalIgnoreCase))
        {
            return "Optionaler Kern";
        }

        if (string.Equals(level, ModuleLevels.Extension, StringComparison.OrdinalIgnoreCase))
        {
            return "Erweiterung";
        }

        return "Kern";
    }

    private static string FormatWorkItemStatus(string? status)
    {
        if (string.Equals(status, ModuleWorkItemStatuses.InProgress, StringComparison.OrdinalIgnoreCase))
        {
            return "In Bearbeitung";
        }

        if (string.Equals(status, ModuleWorkItemStatuses.Done, StringComparison.OrdinalIgnoreCase))
        {
            return "Abgeschlossen";
        }

        if (string.IsNullOrWhiteSpace(status))
        {
            return "Offen";
        }

        return string.Equals(status, ModuleWorkItemStatuses.Open, StringComparison.OrdinalIgnoreCase)
            ? "Offen"
            : status;
    }

    private static string BuildStatusDetail(ModuleOverviewModuleDto module)
    {
        if (module.Status == ModuleStatuses.CoreComplete)
        {
            return $"Kern abgeschlossen ({module.CoreDoneCount} / {module.CoreTotalCount} Kern-Arbeitspunkte erledigt)";
        }

        if (module.Status == ModuleStatuses.ExtendedComplete)
        {
            var doneCount = module.CoreDoneCount + module.OptionalCoreDoneCount;
            var totalCount = module.CoreTotalCount + module.OptionalCoreTotalCount;
            return $"Erweitert abgeschlossen ({doneCount} / {totalCount} Kern + optionaler Kern erledigt)";
        }

        if (module.Status == ModuleStatuses.FullComplete)
        {
            return $"Vollständig abgeschlossen ({module.DoneCount} / {module.TotalCount} Arbeitspunkte erledigt)";
        }

        if (module.Status == ModuleStatuses.InProgress)
        {
            return $"In Bearbeitung ({module.DoneCount} / {module.TotalCount} Arbeitspunkte erledigt)";
        }

        return $"Offen ({module.OpenCount} / {module.TotalCount} Arbeitspunkte offen)";
    }

    private static string FormatAuditScore(ModuleOverviewModuleDto module)
    {
        return module.AuditScorePercent.HasValue
            ? $"{module.AuditScorePercent.Value}%"
            : "Nicht geprüft";
    }

    private static string FormatAuditStatus(string? status)
    {
        var normalized = NormalizeAuditValue(status);

        return normalized switch
        {
            "notchecked" => "Kein Nachweis",
            "verified" => "Vollständig belegt",
            "partiallyverified" => "Unvollständig belegt",
            "failed" => "Kein Nachweis",
            _ => string.IsNullOrWhiteSpace(status) ? "Kein Nachweis" : status!
        };
    }

    private static string FormatAuditPartStatus(string? status)
    {
        var normalized = NormalizeAuditValue(status);

        return normalized switch
        {
            "notchecked" => "Nicht geprüft",
            "found" => "Gefunden",
            "complete" => "Vollständig",
            "incomplete" => "Unvollständig",
            "missing" => "Fehlt",
            _ => string.IsNullOrWhiteSpace(status) ? "Nicht geprüft" : status!
        };
    }

    private static string FormatAuditFlag(string? flag)
    {
        var normalized = NormalizeAuditValue(flag).Replace("_", string.Empty);

        return normalized switch
        {
            "missingcode" => "Code fehlt",
            "mdincomplete" => "Dokumentation unvollständig",
            "missingmd" => "Dokumentation fehlt",
            "missingtests" => "Tests fehlen",
            "workitemsnotverified" => "Workitems nicht vollständig geprüft",
            _ => string.IsNullOrWhiteSpace(flag) ? "Hinweis" : flag!
        };
    }

    private static string FormatAuditIssueSeverity(string? severity)
    {
        var normalized = NormalizeAuditValue(severity);

        return normalized switch
        {
            "warning" => "Warnung",
            "info" => "Hinweis",
            "error" => "Fehler",
            _ => string.IsNullOrWhiteSpace(severity) ? "Hinweis" : severity!
        };
    }

    private static string FormatLastAudit(DateTimeOffset? lastAuditUtc)
    {
        return lastAuditUtc.HasValue
            ? lastAuditUtc.Value.UtcDateTime.ToString("yyyy-MM-dd HH:mm 'UTC'")
            : "Kein Nachweis";
    }

    private static string BuildAuditStatusClass(string? status)
    {
        var normalized = NormalizeAuditValue(status);

        return normalized switch
        {
            "verified" => "audit-status--verified",
            "partiallyverified" => "audit-status--partial",
            "failed" => "audit-status--not-checked",
            _ => "audit-status--not-checked"
        };
    }

    private static string NormalizeAuditValue(string? value)
    {
        return (value ?? string.Empty)
            .Trim()
            .Replace(" ", string.Empty)
            .ToLowerInvariant();
    }

    private sealed class ModuleOverviewState
    {
        private readonly HashSet<string> _expandedGroups = new();
        private readonly HashSet<string> _expandedAudits = new();
        private string? _expandedModuleKey;

        public bool IsGroupExpanded(string key)
        {
            return _expandedGroups.Contains(key);
        }

        public bool IsModuleExpanded(string key)
        {
            return string.Equals(_expandedModuleKey, key, StringComparison.Ordinal);
        }

        public bool IsAuditExpanded(string key)
        {
            return _expandedAudits.Contains(key);
        }

        public void ToggleGroup(string key)
        {
            if (_expandedGroups.Contains(key))
            {
                _expandedGroups.Remove(key);

                if (!string.IsNullOrWhiteSpace(_expandedModuleKey) &&
                    _expandedModuleKey.StartsWith($"{key}:", StringComparison.Ordinal))
                {
                    _expandedModuleKey = null;
                }

                return;
            }

            _expandedGroups.Add(key);
        }

        public void ToggleModule(string groupKey, string moduleKey)
        {
            if (!_expandedGroups.Contains(groupKey))
            {
                _expandedGroups.Add(groupKey);
            }

            if (string.Equals(_expandedModuleKey, moduleKey, StringComparison.Ordinal))
            {
                _expandedModuleKey = null;
                return;
            }

            _expandedModuleKey = moduleKey;
        }

        public void ToggleAudit(string moduleKey)
        {
            if (!_expandedAudits.Add(moduleKey))
            {
                _expandedAudits.Remove(moduleKey);
            }
        }
    }
}
