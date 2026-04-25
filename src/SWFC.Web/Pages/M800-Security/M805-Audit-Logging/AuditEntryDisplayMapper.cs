using System.Text.RegularExpressions;
using SWFC.Application.M800_Security.M805_AuditCompliance.Entries;

internal static class AuditEntryDisplayMapper
{
    public static AuditEntryDisplayModel Map(AuditEntryListItem entry)
    {
        return new AuditEntryDisplayModel(
            Action: GetSimpleAction(entry.Action, entry.ObjectType),
            Timestamp: entry.TimestampUtc.ToLocalTime().ToString("dd.MM.yyyy HH:mm:ss"),
            Module: string.IsNullOrWhiteSpace(entry.Module) ? "System" : entry.Module.Trim(),
            Actor: GetActor(entry),
            Target: GetTarget(entry),
            Object: GetObject(entry),
            Client: GetClient(entry),
            Reason: TranslateReason(entry.Reason));
    }

    public static IReadOnlyList<AuditEntryGroupViewModel> Group(IReadOnlyList<AuditEntryListItem> entries)
    {
        var items = entries
            .Select((entry, index) => new AuditEntryGroupItemViewModel(
                Key: $"audit-item-{index}",
                Entry: entry,
                View: Map(entry),
                GroupKey: GetGroupKey(entry),
                GroupTitle: GetGroupTitle(entry)))
            .ToList();

        return items
            .GroupBy(item => new { item.GroupKey, item.GroupTitle })
            .OrderBy(group => GetGroupOrder(group.Key.GroupKey))
            .Select(group => new AuditEntryGroupViewModel(
                Key: group.Key.GroupKey,
                Title: group.Key.GroupTitle,
                Items: group.ToList()))
            .ToList();
    }

    private static string GetGroupKey(AuditEntryListItem entry)
    {
        if (entry.Action is "LoginSuccess" or "LoginFailed" or "Logout")
        {
            return "authentication";
        }

        if (entry.Action == "AccessDenied")
        {
            return "access";
        }

        if (entry.ObjectType == "User")
        {
            return "users";
        }

        if (entry.ObjectType == "Role")
        {
            return "roles";
        }

        return "system";
    }

    private static string GetGroupTitle(AuditEntryListItem entry)
    {
        return GetGroupKey(entry) switch
        {
            "authentication" => "Authentifizierung",
            "users" => "Benutzer",
            "roles" => "Rollen",
            "access" => "Zugriff und Sicherheit",
            _ => "Systemaktionen"
        };
    }

    private static int GetGroupOrder(string key)
    {
        return key switch
        {
            "authentication" => 0,
            "users" => 1,
            "roles" => 2,
            "access" => 3,
            _ => 4
        };
    }

    private static string GetSimpleAction(string? action, string? objectType)
    {
        if (string.IsNullOrWhiteSpace(action))
        {
            return "Unbekannter Vorgang";
        }

        var normalizedObjectType = objectType?.Trim();
        var normalizedAction = action.Trim().Replace(" ", string.Empty);

        if (normalizedAction.Equals("UserChanged", StringComparison.OrdinalIgnoreCase))
        {
            return "Benutzer geändert";
        }

        if (normalizedAction.Equals("RoleChanged", StringComparison.OrdinalIgnoreCase))
        {
            return "Rolle geändert";
        }

        return normalizedAction switch
        {
            "LoginSuccess" => "Anmeldung erfolgreich",
            "LoginFailed" => "Anmeldung fehlgeschlagen",
            "Logout" => "Abmeldung",
            "Create" when normalizedObjectType == "User" => "Benutzer erstellt",
            "Update" when normalizedObjectType == "User" => "Benutzer geändert",
            "Delete" when normalizedObjectType == "User" => "Benutzer gelöscht",
            "Create" when normalizedObjectType == "Role" => "Rolle erstellt",
            "Update" when normalizedObjectType == "Role" => "Rolle geändert",
            "Delete" when normalizedObjectType == "Role" => "Rolle gelöscht",
            "Create" => "Eintrag erstellt",
            "Update" => "Eintrag geändert",
            "Delete" => "Eintrag gelöscht",
            "AccessDenied" => "Zugriff verweigert",
            _ => SplitPascalCase(action.Trim())
        };
    }

    private static string GetActor(AuditEntryListItem entry)
    {
        if (!string.IsNullOrWhiteSpace(entry.ActorDisplayName))
        {
            return entry.ActorDisplayName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(entry.ActorUserId) && !LooksTechnical(entry.ActorUserId))
        {
            return entry.ActorUserId.Trim();
        }

        return "Unbekannter Benutzer";
    }

    private static string GetTarget(AuditEntryListItem entry)
    {
        if (string.IsNullOrWhiteSpace(entry.TargetUserId))
        {
            return "Kein direktes Ziel";
        }

        if (!string.IsNullOrWhiteSpace(entry.ActorUserId) &&
            string.Equals(entry.TargetUserId, entry.ActorUserId, StringComparison.OrdinalIgnoreCase))
        {
            return "Eigenes Benutzerkonto";
        }

        return LooksTechnical(entry.TargetUserId) ? "Benutzerkonto" : entry.TargetUserId.Trim();
    }

    private static string GetObject(AuditEntryListItem entry)
    {
        if (entry.Action is "LoginSuccess" or "LoginFailed" or "Logout")
        {
            return "Authentifizierung";
        }

        if (string.IsNullOrWhiteSpace(entry.ObjectType))
        {
            return "Nicht angegeben";
        }

        return entry.ObjectType.Trim() switch
        {
            "Authentication" => "Authentifizierung",
            "User" => "Benutzer",
            "Role" => "Rolle",
            "AuditEntry" => "Audit-Eintrag",
            _ => SplitPascalCase(entry.ObjectType.Trim())
        };
    }

    private static string GetClient(AuditEntryListItem entry)
    {
        var parts = new List<string>();
        var browser = GetBrowser(entry.ClientInfo);
        var os = GetOperatingSystem(entry.ClientInfo);

        if (!string.IsNullOrWhiteSpace(browser))
        {
            parts.Add(browser);
        }

        if (!string.IsNullOrWhiteSpace(os))
        {
            parts.Add(os);
        }

        if (!string.IsNullOrWhiteSpace(entry.ClientIp))
        {
            parts.Add(entry.ClientIp.Trim());
        }

        return parts.Count == 0 ? "Nicht angegeben" : string.Join(" · ", parts);
    }

    private static string TranslateReason(string? reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            return string.Empty;
        }

        return reason.Trim() switch
        {
            "Local authentication succeeded." => "Lokale Anmeldung erfolgreich.",
            "Local authentication failed." => "Lokale Anmeldung fehlgeschlagen.",
            _ => reason.Trim()
        };
    }

    private static bool LooksTechnical(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) && Guid.TryParse(value, out _);
    }

    private static string SplitPascalCase(string value)
    {
        return Regex.Replace(value, "([a-z])([A-Z])", "$1 $2");
    }

    private static string? GetBrowser(string? clientInfo)
    {
        if (string.IsNullOrWhiteSpace(clientInfo))
        {
            return null;
        }

        if (clientInfo.Contains("Edg/", StringComparison.OrdinalIgnoreCase)) return "Microsoft Edge";
        if (clientInfo.Contains("Chrome/", StringComparison.OrdinalIgnoreCase)) return "Google Chrome";
        if (clientInfo.Contains("Firefox/", StringComparison.OrdinalIgnoreCase)) return "Mozilla Firefox";

        if (clientInfo.Contains("Safari/", StringComparison.OrdinalIgnoreCase) &&
            !clientInfo.Contains("Chrome/", StringComparison.OrdinalIgnoreCase) &&
            !clientInfo.Contains("Edg/", StringComparison.OrdinalIgnoreCase))
        {
            return "Safari";
        }

        return "Browser";
    }

    private static string? GetOperatingSystem(string? clientInfo)
    {
        if (string.IsNullOrWhiteSpace(clientInfo))
        {
            return null;
        }

        if (clientInfo.Contains("Windows", StringComparison.OrdinalIgnoreCase)) return "Windows";
        if (clientInfo.Contains("Mac OS", StringComparison.OrdinalIgnoreCase) ||
            clientInfo.Contains("Macintosh", StringComparison.OrdinalIgnoreCase)) return "macOS";
        if (clientInfo.Contains("Android", StringComparison.OrdinalIgnoreCase)) return "Android";
        if (clientInfo.Contains("iPhone", StringComparison.OrdinalIgnoreCase) ||
            clientInfo.Contains("iPad", StringComparison.OrdinalIgnoreCase)) return "iOS";
        if (clientInfo.Contains("Linux", StringComparison.OrdinalIgnoreCase)) return "Linux";

        return null;
    }
}

internal sealed record AuditEntryDisplayModel(
    string Action,
    string Timestamp,
    string Module,
    string Actor,
    string Target,
    string Object,
    string Client,
    string Reason);

internal sealed record AuditEntryGroupViewModel(
    string Key,
    string Title,
    IReadOnlyList<AuditEntryGroupItemViewModel> Items);

internal sealed record AuditEntryGroupItemViewModel(
    string Key,
    AuditEntryListItem Entry,
    AuditEntryDisplayModel View,
    string GroupKey,
    string GroupTitle);