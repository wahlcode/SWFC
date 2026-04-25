using System.Linq;
using Microsoft.AspNetCore.Components;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M806_AccessControl.Permissions;
using SWFC.Application.M800_Security.M806_AccessControl.RolePermissions;
using SWFC.Application.M800_Security.M806_AccessControl.Roles;

namespace SWFC.Web.Pages.M800_Security.M806_Access_Control.Roles;

public partial class RoleDetail : ComponentBase
{
    [Inject]
    public ICurrentUserService CurrentUserService { get; set; } = default!;

    [Parameter]
    public Guid? Id { get; set; }

    private bool _loading = true;
    private string? _error;
    private string? _success;
    private SecurityContext _securityContext = new(string.Empty, string.Empty, string.Empty, string.Empty, false);
    private RoleDetailsDto? _role;
    private RoleModel _model = new();
    private List<RolePermissionListItemDto> _permissionItems = new();
    private List<PermissionModuleGroup> _permissionGroups = new();

    private bool IsNew => !Id.HasValue;
    private bool CanEditRoleDefinition => _securityContext.IsDeveloperMode;
    private bool IsRoleMetadataReadOnly => !IsNew && _role?.IsSystemRole == true;

    protected override async Task OnParametersSetAsync()
    {
        _securityContext = await CurrentUserService.GetSecurityContextAsync();
        await LoadAsync(clearSuccess: true);
    }

    private async Task LoadAsync(bool clearSuccess)
    {
        _error = null;

        if (clearSuccess)
        {
            _success = null;
        }

        _loading = true;
        _permissionItems.Clear();
        _permissionGroups.Clear();

        if (IsNew)
        {
            _role = null;
            _model = new RoleModel();
            _loading = false;
            return;
        }

        var roleResult = await GetRolePipeline.ExecuteAsync(new GetRoleByIdQuery(Id!.Value));

        if (!roleResult.IsSuccess || roleResult.Value is null)
        {
            _error = roleResult.Error?.Message ?? "Rolle konnte nicht geladen werden.";
            _loading = false;
            return;
        }

        _role = roleResult.Value;
        _model = new RoleModel
        {
            Name = roleResult.Value.Name,
            Description = roleResult.Value.Description ?? string.Empty
        };

        var rolePermissionsResult = await GetRolePermissionsPipeline.ExecuteAsync(
            new GetRolePermissionsQuery(Id.Value));

        if (rolePermissionsResult.IsSuccess && rolePermissionsResult.Value is not null)
        {
            _permissionItems = rolePermissionsResult.Value.ToList();
        }
        else
        {
            var permissionsResult = await GetPermissionsPipeline.ExecuteAsync(new GetPermissionsQuery());

            if (permissionsResult.IsSuccess && permissionsResult.Value is not null)
            {
                _permissionItems = permissionsResult.Value
                    .Select(x => new RolePermissionListItemDto(
                        x.Id,
                        x.Code,
                        x.Name,
                        x.Description,
                        x.Module,
                        false,
                        x.IsActive))
                    .ToList();
            }
            else
            {
                _error = permissionsResult.Error?.Message ?? "Berechtigungen konnten nicht geladen werden.";
            }
        }

        BuildPermissionGroups();
        _loading = false;
    }

    private void BuildPermissionGroups()
    {
        _permissionGroups = _permissionItems
            .GroupBy(x => x.Module)
            .OrderBy(x => x.Key)
            .Select(group => new PermissionModuleGroup(
                group.Key,
                GetModuleDisplayName(group.Key),
                GetModuleDescription(group.Key),
                group.OrderBy(x => GetPermissionSortOrder(x.PermissionCode))
                    .ThenBy(GetPermissionDisplayName)
                    .ToList()))
            .ToList();
    }

    private void OnPermissionChanged(Guid permissionId, object? value)
    {
        if (!CanEditRoleDefinition)
        {
            return;
        }

        var isChecked = value as bool? == true;
        var index = _permissionItems.FindIndex(x => x.PermissionId == permissionId);

        if (index < 0)
        {
            return;
        }

        var item = _permissionItems[index];
        _permissionItems[index] = item with { IsAssigned = isChecked };
        BuildPermissionGroups();
    }

    private async Task Save()
    {
        _error = null;
        _success = null;

        if (!CanEditRoleDefinition)
        {
            _error = "Rollen und Berechtigungen koennen nur im Developer-Modus geaendert werden.";
            return;
        }

        if (IsNew)
        {
            var createResult = await CreateRolePipeline.ExecuteAsync(
                new CreateRoleCommand(
                    _model.Name,
                    _model.Description,
                    _model.Reason));

            if (createResult.IsSuccess)
            {
                Nav.NavigateTo($"/security/roles/{createResult.Value}");
                return;
            }

            _error = createResult.Error?.Message ?? "Rolle konnte nicht erstellt werden.";
            return;
        }

        var updateResult = await UpdateRolePipeline.ExecuteAsync(
            new UpdateRoleCommand(
                Id!.Value,
                _model.Name,
                _model.Description,
                _model.Reason));

        if (!updateResult.IsSuccess)
        {
            _error = updateResult.Error?.Message ?? "Rolle konnte nicht aktualisiert werden.";
            return;
        }

        var assignedPermissionIds = _permissionItems
            .Where(x => x.IsAssigned)
            .Select(x => x.PermissionId)
            .ToArray();

        var permissionResult = await SetRolePermissionsPipeline.ExecuteAsync(
            new SetRolePermissionsCommand(
                Id.Value,
                assignedPermissionIds,
                _model.Reason));

        if (!permissionResult.IsSuccess)
        {
            _error = permissionResult.Error?.Message ?? "Berechtigungen konnten nicht gespeichert werden.";
            return;
        }

        await LoadAsync(clearSuccess: false);
        _success = "Rolle und Berechtigungen wurden erfolgreich gespeichert.";
    }

    private Task ResetAsync()
    {
        if (IsNew)
        {
            _model = new RoleModel();
            _permissionItems.Clear();
            _permissionGroups.Clear();
            return Task.CompletedTask;
        }

        return LoadAsync(clearSuccess: true);
    }

    private bool CanEditPermission(RolePermissionListItemDto item)
    {
        return CanEditRoleDefinition && item.IsPermissionActive;
    }

    private static string GetModuleDisplayName(string moduleCode) =>
        moduleCode switch
        {
            "M102" => "Organisation",
            "M201" => "Assets und Maschinen",
            "M202" => "Instandhaltung",
            "M204" => "Lager und Bestand",
            "M205" => "Energie",
            "M800" => "Security",
            _ => moduleCode
        };

    private static string GetModuleDescription(string moduleCode) =>
        moduleCode switch
        {
            "M102" => "Rechte fuer Benutzerstammdaten, Organisationseinheiten und Zuordnungen.",
            "M201" => "Rechte fuer Maschinen, Anlagen und technische Struktur.",
            "M202" => "Rechte fuer Wartungsauftraege und Wartungsplaene.",
            "M204" => "Rechte fuer Artikel, Lagerorte, Bewegungen und Reservierungen.",
            "M205" => "Rechte fuer Energiezaehler, Messwerte und Auswertungen.",
            "M800" => "Rechte fuer Rollen, Audit und sicherheitsrelevante Verwaltung.",
            _ => "Modulbezogene Berechtigungen."
        };

    private static string GetPermissionDisplayName(RolePermissionListItemDto item)
    {
        if (!string.IsNullOrWhiteSpace(item.PermissionName))
        {
            return item.PermissionName.Trim();
        }

        return HumanizeIdentifier(item.PermissionCode);
    }

    private static string GetPermissionDescription(RolePermissionListItemDto item)
    {
        if (!string.IsNullOrWhiteSpace(item.PermissionDescription))
        {
            return item.PermissionDescription.Trim();
        }

        return $"Erlaubt die Aktion '{GetPermissionDisplayName(item)}' im Modul {GetModuleDisplayName(item.Module)}.";
    }

    private string GetPermissionAssignmentStatus(RolePermissionListItemDto item)
    {
        if (!CanEditRoleDefinition)
        {
            return item.IsAssigned ? "Der Rolle zugeordnet (Lesemodus)" : "Nicht zugeordnet (Lesemodus)";
        }

        return item.IsAssigned ? "Der Rolle zugeordnet" : "Nicht zugeordnet";
    }

    private static int GetPermissionSortOrder(string code)
    {
        if (code.EndsWith(".read", StringComparison.OrdinalIgnoreCase) || code.EndsWith("read", StringComparison.OrdinalIgnoreCase))
        {
            return 1;
        }

        if (code.EndsWith(".create", StringComparison.OrdinalIgnoreCase) || code.EndsWith("create", StringComparison.OrdinalIgnoreCase))
        {
            return 2;
        }

        if (code.EndsWith(".update", StringComparison.OrdinalIgnoreCase) || code.EndsWith("update", StringComparison.OrdinalIgnoreCase))
        {
            return 3;
        }

        if (code.EndsWith(".write", StringComparison.OrdinalIgnoreCase) || code.EndsWith("write", StringComparison.OrdinalIgnoreCase))
        {
            return 4;
        }

        if (code.EndsWith(".delete", StringComparison.OrdinalIgnoreCase) || code.EndsWith("delete", StringComparison.OrdinalIgnoreCase))
        {
            return 5;
        }

        if (code.EndsWith(".release", StringComparison.OrdinalIgnoreCase) || code.EndsWith("release", StringComparison.OrdinalIgnoreCase))
        {
            return 6;
        }

        return 99;
    }

    private static string HumanizeIdentifier(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value
            .Replace('-', ' ')
            .Replace('.', ' ')
            .Replace('_', ' ');

        return string.Join(
            " ",
            normalized
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(token => char.ToUpperInvariant(token[0]) + token[1..]));
    }

    private sealed class RoleModel
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    private sealed record PermissionModuleGroup(
        string ModuleCode,
        string DisplayName,
        string Description,
        List<RolePermissionListItemDto> Items);
}
