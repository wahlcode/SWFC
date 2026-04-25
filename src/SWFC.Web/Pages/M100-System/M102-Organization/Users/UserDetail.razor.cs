using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using SWFC.Application.M100_System.M102_Organization.Assignments;
using SWFC.Application.M100_System.M102_Organization.CostCenters;
using SWFC.Application.M100_System.M102_Organization.OrganizationUnits;
using SWFC.Application.M100_System.M102_Organization.ShiftModels;
using SWFC.Application.M100_System.M102_Organization.Users;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Pipeline.Execution;
using SWFC.Domain.M100_System.M102_Organization.Users;
using SWFC.Web.Pages.M300_Presentation.M306_Localization.Services;

namespace SWFC.Web.Pages.M100_System.M102_Organization.Users;

public partial class UserDetail
{
    [Parameter]
    public Guid Id { get; set; }

    [Inject]
    private IExecutionPipeline<GetUserByIdQuery, UserDetailsDto> GetUserPipeline { get; set; } = default!;

    [Inject]
    private IExecutionPipeline<GetOrganizationUnitsQuery, IReadOnlyList<OrganizationUnitListItem>> GetOrganizationUnitsPipeline { get; set; } = default!;

    [Inject]
    private IExecutionPipeline<GetCostCentersQuery, IReadOnlyList<CostCenterListItem>> GetCostCentersPipeline { get; set; } = default!;

    [Inject]
    private IExecutionPipeline<GetShiftModelsQuery, IReadOnlyList<ShiftModelListItem>> GetShiftModelsPipeline { get; set; } = default!;

    [Inject]
    private IExecutionPipeline<UpdateUserCommand, bool> UpdateUserPipeline { get; set; } = default!;

    [Inject]
    private IExecutionPipeline<ChangeUserStatusCommand, bool> ChangeUserStatusPipeline { get; set; } = default!;

    [Inject]
    private IExecutionPipeline<AssignOrganizationUnitToUserCommand, bool> AssignOrganizationUnitPipeline { get; set; } = default!;

    [Inject]
    private IExecutionPipeline<RemoveOrganizationUnitFromUserCommand, bool> RemoveOrganizationUnitPipeline { get; set; } = default!;

    [Inject]
    private LocalizationTextProvider TextProvider { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    private bool _isLoading = true;
    private bool _isBusy;
    private string? _error;
    private string? _success;

    private string _cultureName = LocalizationTextProvider.DefaultCultureName;

    private string _title = "User details";
    private string _subtitle = "Manage master data, language and organization assignments";
    private string _backText = "Back";
    private string _reloadText = "Reload";
    private string _loadingText = "Loading user...";
    private string _notFoundText = "User not found";
    private string _masterDataInfoText = "Only organizational user master data and UI language are maintained here. Authentication and permissions remain outside M102.";
    private string _costCenterSelectText = "Select cost center";
    private string _shiftModelSelectText = "Select shift model";
    private string _reasonText = "Reason";
    private string _saveText = "Save";
    private string _statusText = "Status";
    private string _currentStatusText = "Current status";
    private string _statusReasonText = "Reason for status change";
    private string _changeStatusText = "Change status";
    private string _organizationUnitsText = "Organization units";
    private string _organizationUnitsInfoText = "Exactly one active primary assignment is required. Additional assignments are managed as secondary assignments.";
    private string _noOrganizationUnitText = "No organization unit has been assigned yet.";
    private string _primaryText = "Primary";
    private string _removeText = "Remove";
    private string _selectText = "-- select --";
    private string _setPrimaryText = "Set as primary assignment";
    private string _assignText = "Assign";
    private string _historyText = "History";
    private string _noHistoryText = "No history entries exist for this user yet.";
    private string _executedByText = "Executed by";
    private string _summaryText = "Summary";
    private string _languageText = "Language";
    private string _noCostCenterText = "No cost center";
    private string _noShiftModelText = "No shift model";
    private string _organizationUnitCountText = "Organization units";
    private string _updatedText = "User was updated successfully.";
    private string _statusUpdatedText = "User status was updated successfully.";
    private string _organizationAssignedText = "Organization unit was assigned successfully.";
    private string _organizationRemovedText = "Organization unit was removed.";
    private string _removeOrganizationReasonText = "Organization unit removed from user detail.";

    private string _usernameText = "Username";
    private string _displayNameText = "Display Name";
    private string _firstNameText = "First name";
    private string _lastNameText = "Last name";
    private string _employeeNumberText = "Employee number";
    private string _businessEmailText = "Business email";
    private string _businessPhoneText = "Business phone";
    private string _plantText = "Plant";
    private string _locationText = "Location";
    private string _teamText = "Team";
    private string _jobFunctionText = "Job function";
    private string _userTypeText = "Type";

    private UserDetailsDto? _user;
    private IReadOnlyList<OrganizationUnitListItem> _allOrganizationUnits = Array.Empty<OrganizationUnitListItem>();
    private List<OrganizationUnitListItem> _availableOrganizationUnits = new();
    private IReadOnlyList<CostCenterListItem> _costCenters = Array.Empty<CostCenterListItem>();
    private IReadOnlyList<ShiftModelListItem> _shiftModels = Array.Empty<ShiftModelListItem>();

    private EditUserInputModel _editModel = new();
    private ChangeUserStatusInputModel _changeStatusModel = new();
    private AssignOrganizationUnitInputModel _assignOrganizationUnitModel = new();

    protected override async Task OnParametersSetAsync()
    {
        await LoadAsync();
    }

    private async Task ReloadAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        _isLoading = true;
        _error = null;
        _success = null;

        try
        {
            var userResult = await GetUserPipeline.ExecuteAsync(new GetUserByIdQuery(Id));
            var unitResult = await GetOrganizationUnitsPipeline.ExecuteAsync(new GetOrganizationUnitsQuery());
            var costCenterResult = await GetCostCentersPipeline.ExecuteAsync(new GetCostCentersQuery());
            var shiftModelResult = await GetShiftModelsPipeline.ExecuteAsync(new GetShiftModelsQuery());

            if (!userResult.IsSuccess || userResult.Value is null)
            {
                _user = null;
                _error = userResult.Error?.Message;
                return;
            }

            _user = userResult.Value;
            _cultureName = string.IsNullOrWhiteSpace(_user.PreferredCultureName)
                ? LocalizationTextProvider.DefaultCultureName
                : _user.PreferredCultureName.Trim();

            await LocalizeAsync();

            _allOrganizationUnits = unitResult.IsSuccess && unitResult.Value is not null
                ? unitResult.Value
                : Array.Empty<OrganizationUnitListItem>();

            _costCenters = costCenterResult.IsSuccess && costCenterResult.Value is not null
                ? costCenterResult.Value
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
                    .ToArray()
                : Array.Empty<CostCenterListItem>();

            _shiftModels = shiftModelResult.IsSuccess && shiftModelResult.Value is not null
                ? shiftModelResult.Value
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
                    .ToArray()
                : Array.Empty<ShiftModelListItem>();

            ResetEditModel();
            ResetActionModels();
            RebuildAvailableSelections();

            if (!unitResult.IsSuccess && !string.IsNullOrWhiteSpace(unitResult.Error?.Message))
            {
                _error ??= unitResult.Error.Message;
            }

            if (!costCenterResult.IsSuccess && !string.IsNullOrWhiteSpace(costCenterResult.Error?.Message))
            {
                _error ??= costCenterResult.Error.Message;
            }

            if (!shiftModelResult.IsSuccess && !string.IsNullOrWhiteSpace(shiftModelResult.Error?.Message))
            {
                _error ??= shiftModelResult.Error.Message;
            }
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task LocalizeAsync()
    {
        _title = await TextProvider.GetTextAsync("UserDetail.Title", _cultureName);
        _subtitle = await TextProvider.GetTextAsync("UserDetail.Subtitle", _cultureName);
        _backText = await TextProvider.GetTextAsync("Common.Back", _cultureName);
        _reloadText = await TextProvider.GetTextAsync("Common.Reload", _cultureName);
        _loadingText = await TextProvider.GetTextAsync("UserDetail.Loading", _cultureName);
        _notFoundText = await TextProvider.GetTextAsync("UserDetail.NotFound", _cultureName);
        _masterDataInfoText = await TextProvider.GetTextAsync("UserDetail.MasterDataInfo", _cultureName);
        _costCenterSelectText = await TextProvider.GetTextAsync("UserDetail.SelectCostCenter", _cultureName);
        _shiftModelSelectText = await TextProvider.GetTextAsync("UserDetail.SelectShiftModel", _cultureName);
        _reasonText = await TextProvider.GetTextAsync("Common.Reason", _cultureName);
        _saveText = await TextProvider.GetTextAsync("Common.Save", _cultureName);
        _statusText = await TextProvider.GetTextAsync("UserDetail.Status", _cultureName);
        _currentStatusText = await TextProvider.GetTextAsync("UserDetail.CurrentStatus", _cultureName);
        _statusReasonText = await TextProvider.GetTextAsync("UserDetail.StatusReason", _cultureName);
        _changeStatusText = await TextProvider.GetTextAsync("UserDetail.ChangeStatus", _cultureName);
        _organizationUnitsText = await TextProvider.GetTextAsync("UserDetail.OrganizationUnits", _cultureName);
        _organizationUnitsInfoText = await TextProvider.GetTextAsync("UserDetail.OrganizationUnitsInfo", _cultureName);
        _noOrganizationUnitText = await TextProvider.GetTextAsync("UserDetail.NoOrganizationUnit", _cultureName);
        _primaryText = await TextProvider.GetTextAsync("UserDetail.Primary", _cultureName);
        _removeText = await TextProvider.GetTextAsync("Common.Remove", _cultureName);
        _selectText = await TextProvider.GetTextAsync("Common.Select", _cultureName);
        _setPrimaryText = await TextProvider.GetTextAsync("UserDetail.SetPrimary", _cultureName);
        _assignText = await TextProvider.GetTextAsync("Common.Assign", _cultureName);
        _historyText = await TextProvider.GetTextAsync("UserDetail.History", _cultureName);
        _noHistoryText = await TextProvider.GetTextAsync("UserDetail.NoHistory", _cultureName);
        _executedByText = await TextProvider.GetTextAsync("UserDetail.ExecutedBy", _cultureName);
        _summaryText = await TextProvider.GetTextAsync("UserDetail.Summary", _cultureName);
        _languageText = await TextProvider.GetTextAsync("Common.Language", _cultureName);
        _noCostCenterText = await TextProvider.GetTextAsync("UserDetail.NoCostCenter", _cultureName);
        _noShiftModelText = await TextProvider.GetTextAsync("UserDetail.NoShiftModel", _cultureName);
        _organizationUnitCountText = await TextProvider.GetTextAsync("UserDetail.OrganizationUnits", _cultureName);
        _updatedText = await TextProvider.GetTextAsync("UserDetail.Updated", _cultureName);
        _statusUpdatedText = await TextProvider.GetTextAsync("UserDetail.StatusUpdated", _cultureName);
        _organizationAssignedText = await TextProvider.GetTextAsync("UserDetail.OrganizationAssigned", _cultureName);
        _organizationRemovedText = await TextProvider.GetTextAsync("UserDetail.OrganizationRemoved", _cultureName);
        _removeOrganizationReasonText = await TextProvider.GetTextAsync("UserDetail.RemoveOrganizationReason", _cultureName);

        _usernameText = await TextProvider.GetTextAsync("Users.Username", _cultureName);
        _displayNameText = await TextProvider.GetTextAsync("Users.DisplayName", _cultureName);
        _firstNameText = await TextProvider.GetTextAsync("Users.FirstName", _cultureName);
        _lastNameText = await TextProvider.GetTextAsync("Users.LastName", _cultureName);
        _employeeNumberText = await TextProvider.GetTextAsync("Users.EmployeeNumber", _cultureName);
        _businessEmailText = await TextProvider.GetTextAsync("Users.BusinessEmail", _cultureName);
        _businessPhoneText = await TextProvider.GetTextAsync("Users.BusinessPhone", _cultureName);
        _plantText = await TextProvider.GetTextAsync("Users.Plant", _cultureName);
        _locationText = await TextProvider.GetTextAsync("Users.Location", _cultureName);
        _teamText = await TextProvider.GetTextAsync("Users.Team", _cultureName);
        _jobFunctionText = await TextProvider.GetTextAsync("Users.JobFunction", _cultureName);
        _userTypeText = await TextProvider.GetTextAsync("Users.UserType", _cultureName);
    }

    private void ResetEditModel()
    {
        if (_user is null)
        {
            return;
        }

        _editModel = new EditUserInputModel
        {
            Username = _user.Username,
            DisplayName = _user.DisplayName,
            FirstName = _user.FirstName,
            LastName = _user.LastName,
            EmployeeNumber = _user.EmployeeNumber,
            BusinessEmail = _user.BusinessEmail,
            BusinessPhone = _user.BusinessPhone,
            Plant = _user.Plant,
            Location = _user.Location,
            Team = _user.Team,
            CostCenterId = _user.CostCenterId,
            ShiftModelId = _user.ShiftModelId,
            JobFunction = _user.JobFunction,
            PreferredCultureName = string.IsNullOrWhiteSpace(_user.PreferredCultureName)
                ? LocalizationTextProvider.DefaultCultureName
                : _user.PreferredCultureName,
            UserType = _user.UserType,
            Reason = string.Empty
        };
    }

    private void ResetActionModels()
    {
        _assignOrganizationUnitModel = new AssignOrganizationUnitInputModel();
        _changeStatusModel = _user is null
            ? new ChangeUserStatusInputModel()
            : new ChangeUserStatusInputModel
            {
                Status = _user.Status
            };
    }

    private void RebuildAvailableSelections()
    {
        if (_user is null)
        {
            _availableOrganizationUnits = new List<OrganizationUnitListItem>();
            return;
        }

        var assignedOrganizationUnitIds = _user.OrganizationUnits.Select(x => x.Id).ToHashSet();

        _availableOrganizationUnits = _allOrganizationUnits
            .Where(x => !assignedOrganizationUnitIds.Contains(x.Id))
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private async Task UpdateUserAsync()
    {
        if (_user is null)
        {
            return;
        }

        _error = null;
        _success = null;
        _isBusy = true;

        try
        {
            var result = await UpdateUserPipeline.ExecuteAsync(
                new UpdateUserCommand(
                    _user.Id,
                    _editModel.Username,
                    _editModel.DisplayName,
                    _editModel.FirstName,
                    _editModel.LastName,
                    _editModel.EmployeeNumber,
                    _editModel.BusinessEmail,
                    _editModel.BusinessPhone,
                    _editModel.Plant,
                    _editModel.Location,
                    _editModel.Team,
                    _editModel.CostCenterId,
                    _editModel.ShiftModelId,
                    _editModel.JobFunction,
                    _editModel.PreferredCultureName,
                    _editModel.UserType,
                    _editModel.Reason));

            if (!result.IsSuccess)
            {
                _error = result.Error?.Message;
                return;
            }

            _success = _updatedText;
            await LoadAsync();
        }
        finally
        {
            _isBusy = false;
        }
    }

    private async Task ChangeUserStatusAsync()
    {
        if (_user is null)
        {
            return;
        }

        _error = null;
        _success = null;
        _isBusy = true;

        try
        {
            var result = await ChangeUserStatusPipeline.ExecuteAsync(
                new ChangeUserStatusCommand(
                    _user.Id,
                    _changeStatusModel.Status,
                    _changeStatusModel.Reason));

            if (!result.IsSuccess)
            {
                _error = result.Error?.Message;
                return;
            }

            _success = _statusUpdatedText;
            await LoadAsync();
        }
        finally
        {
            _isBusy = false;
        }
    }

    private async Task AssignOrganizationUnitAsync()
    {
        if (_user is null || !_assignOrganizationUnitModel.OrganizationUnitId.HasValue)
        {
            return;
        }

        _error = null;
        _success = null;
        _isBusy = true;

        try
        {
            var result = await AssignOrganizationUnitPipeline.ExecuteAsync(
                new AssignOrganizationUnitToUserCommand(
                    _user.Id,
                    _assignOrganizationUnitModel.OrganizationUnitId.Value,
                    _assignOrganizationUnitModel.IsPrimary,
                    _assignOrganizationUnitModel.Reason));

            if (!result.IsSuccess)
            {
                _error = result.Error?.Message;
                return;
            }

            _success = _organizationAssignedText;
            await LoadAsync();
        }
        finally
        {
            _isBusy = false;
        }
    }

    private async Task RemoveOrganizationUnitAsync(Guid organizationUnitId)
    {
        if (_user is null)
        {
            return;
        }

        _error = null;
        _success = null;
        _isBusy = true;

        try
        {
            var result = await RemoveOrganizationUnitPipeline.ExecuteAsync(
                new RemoveOrganizationUnitFromUserCommand(
                    _user.Id,
                    organizationUnitId,
                    _removeOrganizationReasonText));

            if (!result.IsSuccess)
            {
                _error = result.Error?.Message;
                return;
            }

            _success = _organizationRemovedText;
            await LoadAsync();
        }
        finally
        {
            _isBusy = false;
        }
    }

    private sealed class EditUserInputModel
    {
        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Display name is required.")]
        public string DisplayName { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Employee number is required.")]
        public string EmployeeNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Business email is required.")]
        public string BusinessEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Business phone is required.")]
        public string BusinessPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Plant is required.")]
        public string Plant { get; set; } = string.Empty;

        [Required(ErrorMessage = "Location is required.")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Team is required.")]
        public string Team { get; set; } = string.Empty;

        public Guid? CostCenterId { get; set; }

        public Guid? ShiftModelId { get; set; }

        [Required(ErrorMessage = "Job function is required.")]
        public string JobFunction { get; set; } = string.Empty;

        [Required(ErrorMessage = "Language is required.")]
        public string PreferredCultureName { get; set; } = LocalizationTextProvider.DefaultCultureName;

        public UserType UserType { get; set; } = UserType.Internal;

        [Required(ErrorMessage = "Reason is required.")]
        public string Reason { get; set; } = string.Empty;
    }

    private sealed class ChangeUserStatusInputModel
    {
        public UserStatus Status { get; set; } = UserStatus.Active;

        [Required(ErrorMessage = "Reason is required.")]
        public string Reason { get; set; } = string.Empty;
    }

    private sealed class AssignOrganizationUnitInputModel
    {
        [Required(ErrorMessage = "Organization unit is required.")]
        public Guid? OrganizationUnitId { get; set; }

        public bool IsPrimary { get; set; }

        [Required(ErrorMessage = "Reason is required.")]
        public string Reason { get; set; } = string.Empty;
    }
}