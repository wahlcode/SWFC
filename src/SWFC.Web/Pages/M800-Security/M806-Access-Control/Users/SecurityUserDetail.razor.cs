using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using SWFC.Application.M100_System.M103_Authentication;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Pipeline.Execution;
using SWFC.Application.M800_Security.M806_AccessControl.Assignments;
using SWFC.Application.M800_Security.M806_AccessControl.Roles;
using SWFC.Application.M800_Security.M806_AccessControl.Users;
using SWFC.Infrastructure.M100_System.M107_SetupDeployment;

namespace SWFC.Web.Pages.M800_Security.M806_Access_Control.Users;

public partial class SecurityUserDetail
{
    [Inject]
    public IOptions<M107SetupOptions> SetupOptionsAccessor { get; set; } = default!;

    [Parameter]
    public Guid Id { get; set; }

    private bool _loading = true;
    private bool _busy;
    private string? _error;
    private string? _success;
    private SecurityContext _currentSecurityContext = new(string.Empty, string.Empty, string.Empty, string.Empty, false);
    private IReadOnlyList<SecurityUserListItemDto> _users = Array.Empty<SecurityUserListItemDto>();
    private IReadOnlyList<RoleListItem> _roles = Array.Empty<RoleListItem>();
    private SecurityUserDetailsDto? _selectedUser;
    private IReadOnlyList<RoleListItem> _assignableRoles = Array.Empty<RoleListItem>();
    private RoleAssignmentModel _roleAssignmentModel = new();
    private PasswordResetModel _passwordResetModel = new();
    private string AdminRoleName => SetupOptionsAccessor.Value.AdminRoleName;
    private string SuperAdminRoleName => SetupOptionsAccessor.Value.SuperAdminRoleName;
    private string LegacyDeveloperRoleName => SetupOptionsAccessor.Value.LegacyDeveloperRoleName;
    private bool IsCurrentUserSuperAdmin => _currentSecurityContext.HasRole(SuperAdminRoleName);
    private bool IsCurrentUserAdmin => _currentSecurityContext.HasRole(AdminRoleName);
    private bool CanManageRoleAssignments =>
        _selectedUser is not null &&
        (_currentSecurityContext.IsDeveloperMode ||
         (IsCurrentUserSuperAdmin && !_selectedUser.IsProtectedSuperAdmin));
    private bool CanResetSelectedPassword =>
        _selectedUser is not null &&
        (_currentSecurityContext.IsDeveloperMode ||
         (IsCurrentUserSuperAdmin && !_selectedUser.IsProtectedSuperAdmin) ||
         (IsCurrentUserAdmin && !TargetHasPrivilegedRole(_selectedUser)));
    private string? RoleManagementNotice =>
        _selectedUser is null
            ? null
            : _currentSecurityContext.IsDeveloperMode
                ? "Developer-Modus ist aktiv. Alle zulaessigen Rollenoperationen sind verfuegbar."
                : _selectedUser.IsProtectedSuperAdmin
                    ? "Der geschuetzte SuperAdmin darf ausserhalb des Developer-Modus nicht geaendert werden."
                    : IsCurrentUserSuperAdmin
                        ? "SuperAdmin darf Benutzerrollen verwalten, aber keine SuperAdmin-Zuweisung aendern."
                        : "Rollen koennen nur durch SuperAdmin oder im Developer-Modus geaendert werden.";
    private string? PasswordManagementNotice =>
        _selectedUser is null
            ? null
            : _currentSecurityContext.IsDeveloperMode
                ? "Developer-Modus erlaubt Passwortverwaltung ohne zusaetzliche Einschraenkung."
                : _selectedUser.IsProtectedSuperAdmin
                    ? "Der geschuetzte SuperAdmin setzt sein Passwort selbst oder im Developer-Modus."
                    : IsCurrentUserSuperAdmin
                        ? "SuperAdmin darf Passwoerter anderer Benutzer setzen."
                        : IsCurrentUserAdmin
                            ? "Admins duerfen nur Passwoerter nicht privilegierter Benutzer setzen."
                            : "Passwortaenderungen sind fuer diesen Benutzer nicht freigegeben.";

    protected override async Task OnParametersSetAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        _loading = true;
        _error = null;

        try
        {
            _currentSecurityContext = await CurrentUserService.GetSecurityContextAsync();

            var usersResult = await GetSecurityUsersPipeline.ExecuteAsync(new GetSecurityUsersQuery());
            var userResult = await GetSecurityUserPipeline.ExecuteAsync(new GetSecurityUserByIdQuery(Id));
            var rolesResult = await GetRolesPipeline.ExecuteAsync(new GetRolesQuery());

            if (!usersResult.IsSuccess || usersResult.Value is null)
            {
                _error = usersResult.Error?.Message ?? "Sicherheitsbenutzer konnten nicht geladen werden.";
                return;
            }

            if (!userResult.IsSuccess || userResult.Value is null)
            {
                _error = userResult.Error?.Message ?? "Sicherheitsdaten konnten nicht geladen werden.";
                return;
            }

            if (!rolesResult.IsSuccess || rolesResult.Value is null)
            {
                _error = rolesResult.Error?.Message ?? "Rollen konnten nicht geladen werden.";
                return;
            }

            _users = usersResult.Value;
            _selectedUser = userResult.Value;
            _roles = rolesResult.Value
                .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            RebuildAssignableRoles();
            _roleAssignmentModel = new RoleAssignmentModel();
            _passwordResetModel = new PasswordResetModel();
        }
        finally
        {
            _loading = false;
        }
    }

    private void RebuildAssignableRoles()
    {
        if (_selectedUser is null)
        {
            _assignableRoles = Array.Empty<RoleListItem>();
            return;
        }

        var assignedRoles = _selectedUser.Roles.ToHashSet(StringComparer.OrdinalIgnoreCase);

        _assignableRoles = _roles
            .Where(x => x.IsActive && !assignedRoles.Contains(x.Name) && CanAssignRole(x.Name))
            .ToArray();
    }

    private async Task AssignRoleAsync()
    {
        if (!CanManageRoleAssignments)
        {
            _error = RoleManagementNotice ?? "Rollen koennen fuer diesen Benutzer nicht geaendert werden.";
            return;
        }

        if (_selectedUser is null || !_roleAssignmentModel.RoleId.HasValue)
        {
            _error = "Bitte zuerst eine Rolle auswählen.";
            return;
        }

        var selectedRole = _roles.FirstOrDefault(x => x.Id == _roleAssignmentModel.RoleId.Value);

        if (selectedRole is null || !CanAssignRole(selectedRole.Name))
        {
            _error = "Diese Rolle darf im aktuellen Modus nicht zugewiesen werden.";
            return;
        }

        if (string.IsNullOrWhiteSpace(_roleAssignmentModel.Reason))
        {
            _error = "Eine Begründung ist erforderlich.";
            return;
        }

        await RunBusyActionAsync(async () =>
        {
            var result = await AssignRoleToUserPipeline.ExecuteAsync(
                new AssignRoleToUserCommand(
                    _selectedUser.Id,
                    _roleAssignmentModel.RoleId.Value,
                    _roleAssignmentModel.Reason));

            if (!result.IsSuccess)
            {
                _error = result.Error?.Message ?? "Rolle konnte nicht zugewiesen werden.";
                return;
            }

            _success = "Rolle wurde erfolgreich zugewiesen.";
            await LoadAsync();
        });
    }

    private async Task RemoveRoleAsync(string roleName)
    {
        if (!CanManageRole(roleName))
        {
            _error = RoleManagementNotice ?? "Diese Rolle darf im aktuellen Modus nicht entfernt werden.";
            return;
        }

        if (_selectedUser is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_roleAssignmentModel.Reason))
        {
            _error = "Für Rollenänderungen ist eine Begründung erforderlich.";
            return;
        }

        var role = _roles.FirstOrDefault(x => string.Equals(x.Name, roleName, StringComparison.OrdinalIgnoreCase));

        if (role is null)
        {
            _error = "Rolle konnte nicht aufgelöst werden.";
            return;
        }

        await RunBusyActionAsync(async () =>
        {
            var result = await RemoveRoleFromUserPipeline.ExecuteAsync(
                new RemoveRoleFromUserCommand(
                    _selectedUser.Id,
                    role.Id,
                    _roleAssignmentModel.Reason));

            if (!result.IsSuccess)
            {
                _error = result.Error?.Message ?? "Rolle konnte nicht entfernt werden.";
                return;
            }

            _success = "Rolle wurde erfolgreich entfernt.";
            await LoadAsync();
        });
    }

    private async Task ResetPasswordAsync()
    {
        if (!CanResetSelectedPassword)
        {
            _error = PasswordManagementNotice ?? "Das Passwort darf fuer diesen Benutzer nicht gesetzt werden.";
            return;
        }

        if (_selectedUser is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_passwordResetModel.NewPassword))
        {
            _error = "Ein neues Passwort ist erforderlich.";
            return;
        }

        if (!string.Equals(_passwordResetModel.NewPassword, _passwordResetModel.ConfirmPassword, StringComparison.Ordinal))
        {
            _error = "Die Passwortbestätigung stimmt nicht überein.";
            return;
        }

        await RunBusyActionAsync(async () =>
        {
            var result = await AdminSetUserPasswordHandler.HandleAsync(
                new AdminSetUserPasswordCommand(
                    _selectedUser.Id,
                    _passwordResetModel.NewPassword));

            if (result.IsFailure || !result.Value)
            {
                _error = result.Error?.Message ?? "Passwort konnte nicht gesetzt werden.";
                return;
            }

            _success = "Passwort wurde erfolgreich gesetzt.";
            await LoadAsync();
        });
    }

    private async Task RunBusyActionAsync(Func<Task> action)
    {
        _busy = true;
        _error = null;
        _success = null;

        try
        {
            await action();
        }
        finally
        {
            _busy = false;
        }
    }

    private bool CanAssignRole(string roleName)
    {
        if (!CanManageRoleAssignments)
        {
            return false;
        }

        if (_currentSecurityContext.IsDeveloperMode)
        {
            return true;
        }

        return !IsDeveloperOnlyRole(roleName);
    }

    private bool CanManageRole(string roleName)
    {
        if (_selectedUser is null || !CanManageRoleAssignments)
        {
            return false;
        }

        if (_currentSecurityContext.IsDeveloperMode)
        {
            return true;
        }

        if (string.Equals(roleName, SuperAdminRoleName, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return !string.Equals(roleName, LegacyDeveloperRoleName, StringComparison.OrdinalIgnoreCase);
    }

    private bool TargetHasPrivilegedRole(SecurityUserDetailsDto user)
    {
        return user.Roles.Any(role =>
            string.Equals(role, AdminRoleName, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role, SuperAdminRoleName, StringComparison.OrdinalIgnoreCase));
    }

    private bool IsDeveloperOnlyRole(string roleName)
    {
        return string.Equals(roleName, SuperAdminRoleName, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(roleName, LegacyDeveloperRoleName, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class RoleAssignmentModel
    {
        public Guid? RoleId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    private sealed class PasswordResetModel
    {
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
