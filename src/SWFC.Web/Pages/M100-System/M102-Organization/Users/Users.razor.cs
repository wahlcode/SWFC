using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using SWFC.Application.M100_System.M102_Organization.CostCenters;
using SWFC.Application.M100_System.M102_Organization.OrganizationUnits;
using SWFC.Application.M100_System.M102_Organization.ShiftModels;
using SWFC.Application.M100_System.M102_Organization.Users;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Pipeline.Execution;
using SWFC.Domain.M100_System.M102_Organization.Users;
using SWFC.Web.Pages.M300_Presentation.M306_Localization.Services;

namespace SWFC.Web.Pages.M100_System.M102_Organization.Users;

public partial class Users
{
    [Inject]
    private IExecutionPipeline<GetUsersQuery, IReadOnlyList<UserListItem>> GetUsersPipeline { get; set; } = default!;

    [Inject]
    private IExecutionPipeline<GetUserByIdQuery, UserDetailsDto> GetUserByIdPipeline { get; set; } = default!;

    [Inject]
    private IExecutionPipeline<GetOrganizationUnitsQuery, IReadOnlyList<OrganizationUnitListItem>> GetOrganizationUnitsPipeline { get; set; } = default!;

    [Inject]
    private IExecutionPipeline<GetCostCentersQuery, IReadOnlyList<CostCenterListItem>> GetCostCentersPipeline { get; set; } = default!;

    [Inject]
    private IExecutionPipeline<GetShiftModelsQuery, IReadOnlyList<ShiftModelListItem>> GetShiftModelsPipeline { get; set; } = default!;

    [Inject]
    private IExecutionPipeline<CreateUserCommand, Guid> CreateUserPipeline { get; set; } = default!;

    [Inject]
    private ICurrentUserService CurrentUserService { get; set; } = default!;

    [Inject]
    private LocalizationTextProvider TextProvider { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    private bool _isLoading = true;
    private bool _isSubmitting;
    private bool _showCreatePanel;
    private bool _isDetailLoading;

    private string? _error;
    private string? _success;
    private string? _detailError;

    private string _cultureName = LocalizationTextProvider.DefaultCultureName;

    private string _backText = "Back";
    private string _eyebrowText = "M102 · Organization";
    private string _titleText = "Users";
    private string _subtitleText = "Internal and external people, organizational assignment and master data management.";
    private string _filteredText = "Filtered";
    private string _activeText = "Active";
    private string _externalText = "External";
    private string _withShiftModelText = "With shift model";
    private string _loadingText = "Loading user data...";
    private string _searchLabelText = "Search users...";
    private string _onlyActiveText = "Only active users";
    private string _createUserText = "Create user";
    private string _userListText = "User list";
    private string _userListSubtitleText = "Grouped by real primary organization unit. The detail panel uses real M102 data of the selected user.";
    private string _noUsersTitleText = "No users found";
    private string _noUsersText = "Check search and filters or create a new user.";
    private string _noSelectedUserTitleText = "No user selected";
    private string _noSelectedUserText = "Select a user on the left to view details and assignments.";
    private string _userDetailsText = "User details";
    private string _detailLoadingText = "Loading user details...";
    private string _detailUnavailableText = "Detail data is currently unavailable.";
    private string _masterDataText = "Master data";
    private string _organizationText = "Organization";
    private string _organizationUnitsText = "Organization units";
    private string _noOrganizationUnitText = "No organization unit assigned.";
    private string _fullDetailPageText = "Full detail page";
    private string _notMaintainedText = "Not maintained";
    private string _createdText = "User created.";
    private string _createFailedText = "User could not be created.";
    private string _primaryText = "Primary";

    private string _displayNameText = "Display Name";
    private string _usernameText = "Username";
    private string _employeeNumberText = "Employee number";
    private string _emailText = "Email";
    private string _businessPhoneText = "Business phone";
    private string _jobFunctionText = "Job function";
    private string _plantText = "Plant";
    private string _locationText = "Location";
    private string _teamText = "Team";
    private string _costCenterText = "Cost center";
    private string _shiftModelText = "Shift model";
    private string _identityKeyText = "Identity key";
    private string _personnelNumberPrefixText = "PN";
    private string _noPrimaryOrganizationText = "Without primary organization unit";

    private string _searchText = string.Empty;
    private bool _onlyActive;
    private Guid? _selectedUserId;
    private Guid? _loadedUserDetailsId;
    private readonly HashSet<string> _expandedGroups = new(StringComparer.OrdinalIgnoreCase);

    private IReadOnlyList<UserListItem> _users = [];
    private List<UserListItem> _filteredUsers = [];
    private IReadOnlyList<OrganizationUnitListItem> _organizationUnits = Array.Empty<OrganizationUnitListItem>();
    private IReadOnlyList<CostCenterListItem> _costCenters = Array.Empty<CostCenterListItem>();
    private IReadOnlyList<ShiftModelListItem> _shiftModels = Array.Empty<ShiftModelListItem>();
    private readonly Dictionary<Guid, UserDetailsDto> _userDetailsCache = new();

    private UserDetailsDto? _selectedUserDetails;
    private CreateUserModel _createModel = new();
    private EditContext _createEditContext = default!;

    protected override async Task OnInitializedAsync()
    {
        _createEditContext = new EditContext(_createModel);

        var securityContext = await CurrentUserService.GetSecurityContextAsync();
        _cultureName = string.IsNullOrWhiteSpace(securityContext.PreferredCultureName)
            ? LocalizationTextProvider.DefaultCultureName
            : securityContext.PreferredCultureName.Trim();

        await LocalizeAsync();
        await LoadAsync();
    }

    private async Task LocalizeAsync()
    {
        _backText = await TextProvider.GetTextAsync("Common.Back", _cultureName);
        _eyebrowText = await TextProvider.GetTextAsync("Users.Eyebrow", _cultureName);
        _titleText = await TextProvider.GetTextAsync("Users.Title", _cultureName);
        _subtitleText = await TextProvider.GetTextAsync("Users.Subtitle", _cultureName);
        _filteredText = await TextProvider.GetTextAsync("Users.Filtered", _cultureName);
        _activeText = await TextProvider.GetTextAsync("Users.Active", _cultureName);
        _externalText = await TextProvider.GetTextAsync("Users.External", _cultureName);
        _withShiftModelText = await TextProvider.GetTextAsync("Users.WithShiftModel", _cultureName);
        _loadingText = await TextProvider.GetTextAsync("Users.Loading", _cultureName);
        _searchLabelText = await TextProvider.GetTextAsync("Users.SearchLabel", _cultureName);
        _onlyActiveText = await TextProvider.GetTextAsync("Users.OnlyActive", _cultureName);
        _createUserText = await TextProvider.GetTextAsync("Users.CreateUser", _cultureName);
        _userListText = await TextProvider.GetTextAsync("Users.UserList", _cultureName);
        _userListSubtitleText = await TextProvider.GetTextAsync("Users.UserListSubtitle", _cultureName);
        _noUsersTitleText = await TextProvider.GetTextAsync("Users.NoUsersTitle", _cultureName);
        _noUsersText = await TextProvider.GetTextAsync("Users.NoUsersText", _cultureName);
        _noSelectedUserTitleText = await TextProvider.GetTextAsync("Users.NoSelectedUserTitle", _cultureName);
        _noSelectedUserText = await TextProvider.GetTextAsync("Users.NoSelectedUserText", _cultureName);
        _userDetailsText = await TextProvider.GetTextAsync("Users.UserDetails", _cultureName);
        _detailLoadingText = await TextProvider.GetTextAsync("Users.DetailLoading", _cultureName);
        _detailUnavailableText = await TextProvider.GetTextAsync("Users.DetailUnavailable", _cultureName);
        _masterDataText = await TextProvider.GetTextAsync("Users.MasterData", _cultureName);
        _organizationText = await TextProvider.GetTextAsync("Users.Organization", _cultureName);
        _organizationUnitsText = await TextProvider.GetTextAsync("Users.OrganizationUnits", _cultureName);
        _noOrganizationUnitText = await TextProvider.GetTextAsync("Users.NoOrganizationUnit", _cultureName);
        _fullDetailPageText = await TextProvider.GetTextAsync("Users.FullDetailPage", _cultureName);
        _notMaintainedText = await TextProvider.GetTextAsync("Users.NotMaintained", _cultureName);
        _createdText = await TextProvider.GetTextAsync("Users.Created", _cultureName);
        _createFailedText = await TextProvider.GetTextAsync("Users.CreateFailed", _cultureName);
        _primaryText = await TextProvider.GetTextAsync("Users.Primary", _cultureName);

        _displayNameText = await TextProvider.GetTextAsync("Users.DisplayName", _cultureName);
        _usernameText = await TextProvider.GetTextAsync("Users.Username", _cultureName);
        _employeeNumberText = await TextProvider.GetTextAsync("Users.EmployeeNumber", _cultureName);
        _emailText = await TextProvider.GetTextAsync("Users.Email", _cultureName);
        _businessPhoneText = await TextProvider.GetTextAsync("Users.BusinessPhone", _cultureName);
        _jobFunctionText = await TextProvider.GetTextAsync("Users.JobFunction", _cultureName);
        _plantText = await TextProvider.GetTextAsync("Users.Plant", _cultureName);
        _locationText = await TextProvider.GetTextAsync("Users.Location", _cultureName);
        _teamText = await TextProvider.GetTextAsync("Users.Team", _cultureName);
        _costCenterText = await TextProvider.GetTextAsync("Users.CostCenter", _cultureName);
        _shiftModelText = await TextProvider.GetTextAsync("Users.ShiftModel", _cultureName);
        _identityKeyText = await TextProvider.GetTextAsync("Users.IdentityKey", _cultureName);
        _personnelNumberPrefixText = await TextProvider.GetTextAsync("Users.PersonnelNumberPrefix", _cultureName);
        _noPrimaryOrganizationText = await TextProvider.GetTextAsync("Users.NoPrimaryOrganization", _cultureName);
    }

    private async Task LoadAsync()
    {
        _isLoading = true;
        _error = null;
        _detailError = null;
        _selectedUserDetails = null;
        _loadedUserDetailsId = null;
        _userDetailsCache.Clear();
        _expandedGroups.Clear();

        try
        {
            var usersResult = await GetUsersPipeline.ExecuteAsync(new GetUsersQuery());
            var organizationUnitResult = await GetOrganizationUnitsPipeline.ExecuteAsync(new GetOrganizationUnitsQuery());
            var costCenterResult = await GetCostCentersPipeline.ExecuteAsync(new GetCostCentersQuery());
            var shiftModelResult = await GetShiftModelsPipeline.ExecuteAsync(new GetShiftModelsQuery());

            if (usersResult.IsSuccess && usersResult.Value is not null)
            {
                _users = usersResult.Value;
            }
            else
            {
                _users = Array.Empty<UserListItem>();
                _error = usersResult.Error?.Message;
            }

            if (organizationUnitResult.IsSuccess && organizationUnitResult.Value is not null)
            {
                _organizationUnits = organizationUnitResult.Value
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }
            else if (_error is null)
            {
                _error = organizationUnitResult.Error?.Message;
            }

            if (costCenterResult.IsSuccess && costCenterResult.Value is not null)
            {
                _costCenters = costCenterResult.Value
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }
            else if (_error is null)
            {
                _error = costCenterResult.Error?.Message;
            }

            if (shiftModelResult.IsSuccess && shiftModelResult.Value is not null)
            {
                _shiftModels = shiftModelResult.Value
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }
            else if (_error is null)
            {
                _error = shiftModelResult.Error?.Message;
            }

            await PrimeUserDetailsCacheAsync(_users);
            ApplyFilterCore();
        }
        finally
        {
            _isLoading = false;
        }

        await LoadSelectedUserDetailsAsync();
    }

    private void ApplyFilterCore()
    {
        IEnumerable<UserListItem> query = _users;

        if (!string.IsNullOrWhiteSpace(_searchText))
        {
            var searchText = _searchText.Trim();

            query = query.Where(x =>
                x.IdentityKey.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                x.Username.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                x.DisplayName.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                x.EmployeeNumber.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                x.BusinessEmail.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                x.PreferredCultureName.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                MatchesDetailSearch(x.Id, searchText) ||
                (!string.IsNullOrWhiteSpace(x.CostCenterName) && x.CostCenterName.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrWhiteSpace(x.CostCenterCode) && x.CostCenterCode.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrWhiteSpace(x.ShiftModelName) && x.ShiftModelName.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrWhiteSpace(x.ShiftModelCode) && x.ShiftModelCode.Contains(searchText, StringComparison.OrdinalIgnoreCase)));
        }

        if (_onlyActive)
        {
            query = query.Where(x => x.Status == UserStatus.Active);
        }

        _filteredUsers = query
            .OrderBy(x => GetPrimaryOrganizationGroupLabel(x.Id), StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.Username, StringComparer.OrdinalIgnoreCase)
            .ToList();

        SyncExpandedGroups();

        if (_filteredUsers.Count == 0)
        {
            _selectedUserId = null;
            return;
        }

        if (!_selectedUserId.HasValue)
        {
            _selectedUserId = _filteredUsers[0].Id;
            return;
        }

        if (_filteredUsers.All(x => x.Id != _selectedUserId.Value))
        {
            _selectedUserId = _filteredUsers[0].Id;
        }
    }

    private async Task ApplyFilterAsync()
    {
        ApplyFilterCore();
        await LoadSelectedUserDetailsAsync();
    }

    private UserListItem? SelectedUser =>
        _filteredUsers.FirstOrDefault(x => x.Id == _selectedUserId);

    private IReadOnlyList<UserGroupViewModel> GroupedUsers =>
        _filteredUsers
            .GroupBy(x => GetPrimaryOrganizationGroupLabel(x.Id), StringComparer.OrdinalIgnoreCase)
            .Select(g => new UserGroupViewModel(
                g.Key,
                g.OrderBy(x => x.DisplayName, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(x => x.Username, StringComparer.OrdinalIgnoreCase)
                    .ToList()))
            .OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
            .ToList();

    private async Task SelectUserAsync(Guid userId)
    {
        if (_selectedUserId == userId)
        {
            await LoadSelectedUserDetailsAsync();
            return;
        }

        _selectedUserId = userId;
        await LoadSelectedUserDetailsAsync();
    }

    private void OpenCreatePanel()
    {
        _error = null;
        _success = null;
        _isSubmitting = false;

        _createModel = new CreateUserModel();
        _createEditContext = new EditContext(_createModel);

        _showCreatePanel = true;
    }

    private void CloseCreatePanel()
    {
        if (_isSubmitting)
        {
            return;
        }

        _showCreatePanel = false;
        _error = null;
    }

    private async Task CreateUserAsync()
    {
        if (_isSubmitting)
        {
            return;
        }

        _error = null;
        _success = null;
        _isSubmitting = true;

        try
        {
            var result = await CreateUserPipeline.ExecuteAsync(
                new CreateUserCommand(
                    _createModel.IdentityKey.Trim(),
                    _createModel.Username.Trim(),
                    _createModel.DisplayName.Trim(),
                    _createModel.FirstName.Trim(),
                    _createModel.LastName.Trim(),
                    _createModel.EmployeeNumber.Trim(),
                    _createModel.BusinessEmail.Trim(),
                    _createModel.BusinessPhone.Trim(),
                    _createModel.Plant.Trim(),
                    _createModel.Location.Trim(),
                    _createModel.Team.Trim(),
                    _createModel.CostCenterId,
                    _createModel.ShiftModelId,
                    _createModel.JobFunction.Trim(),
                    _createModel.PreferredCultureName.Trim(),
                    _createModel.PrimaryOrganizationUnitId.GetValueOrDefault(),
                    _createModel.Status,
                    _createModel.UserType,
                    _createModel.Reason.Trim()));

            if (!result.IsSuccess)
            {
                _error = result.Error?.Message ?? _createFailedText;
                return;
            }

            _success = _createdText;
            _showCreatePanel = false;

            await LoadAsync();

            NavigationManager.NavigateTo($"/system/users/{result.Value}");
        }
        finally
        {
            _isSubmitting = false;
        }
    }

    private async Task LoadSelectedUserDetailsAsync(bool forceReload = false)
    {
        _detailError = null;

        if (!_selectedUserId.HasValue)
        {
            _selectedUserDetails = null;
            _loadedUserDetailsId = null;
            return;
        }

        if (!forceReload &&
            _loadedUserDetailsId == _selectedUserId &&
            _selectedUserDetails is not null)
        {
            return;
        }

        _isDetailLoading = true;
        _selectedUserDetails = null;

        try
        {
            if (!forceReload &&
                _userDetailsCache.TryGetValue(_selectedUserId.Value, out var cachedDetails))
            {
                _selectedUserDetails = cachedDetails;
                _loadedUserDetailsId = cachedDetails.Id;
                return;
            }

            var result = await GetUserByIdPipeline.ExecuteAsync(new GetUserByIdQuery(_selectedUserId.Value));

            if (!result.IsSuccess || result.Value is null)
            {
                _loadedUserDetailsId = null;
                _detailError = result.Error?.Message ?? _detailLoadingText;
                return;
            }

            _userDetailsCache[result.Value.Id] = result.Value;
            _selectedUserDetails = result.Value;
            _loadedUserDetailsId = result.Value.Id;
        }
        finally
        {
            _isDetailLoading = false;
        }
    }

    private string DisplayOrFallback(string? value) =>
        string.IsNullOrWhiteSpace(value) ? _notMaintainedText : value;

    private static string? BuildReferenceLabel(string? name, string? code)
    {
        if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(code))
        {
            return $"{name} ({code})";
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            return name;
        }

        if (!string.IsNullOrWhiteSpace(code))
        {
            return code;
        }

        return null;
    }

    private string BuildListItemContext(UserListItem user)
    {
        var parts = new List<string>();

        if (_userDetailsCache.TryGetValue(user.Id, out var details) &&
            !string.IsNullOrWhiteSpace(details.JobFunction))
        {
            parts.Add(details.JobFunction);
        }

        if (!string.IsNullOrWhiteSpace(user.PreferredCultureName))
        {
            parts.Add(user.PreferredCultureName);
        }

        if (!string.IsNullOrWhiteSpace(user.EmployeeNumber))
        {
            parts.Add($"{_personnelNumberPrefixText} {user.EmployeeNumber}");
        }

        var costCenter = BuildReferenceLabel(user.CostCenterName, user.CostCenterCode);
        if (!string.IsNullOrWhiteSpace(costCenter))
        {
            parts.Add(costCenter);
        }

        var shiftModel = BuildReferenceLabel(user.ShiftModelName, user.ShiftModelCode);
        if (!string.IsNullOrWhiteSpace(shiftModel))
        {
            parts.Add(shiftModel);
        }

        if (parts.Count == 0)
        {
            parts.Add(user.IdentityKey);
        }

        return string.Join(" | ", parts);
    }

    private string BuildOrganizationUnitLabel(OrganizationUnitReference organizationUnit)
    {
        var label = $"{organizationUnit.Name} ({organizationUnit.Code})";
        return organizationUnit.IsPrimary ? $"{label} | {_primaryText}" : label;
    }

    private async Task PrimeUserDetailsCacheAsync(IEnumerable<UserListItem> users)
    {
        foreach (var user in users)
        {
            if (_userDetailsCache.ContainsKey(user.Id))
            {
                continue;
            }

            var result = await GetUserByIdPipeline.ExecuteAsync(new GetUserByIdQuery(user.Id));

            if (result.IsSuccess && result.Value is not null)
            {
                _userDetailsCache[user.Id] = result.Value;
            }
        }
    }

    private bool MatchesDetailSearch(Guid userId, string searchText)
    {
        if (!_userDetailsCache.TryGetValue(userId, out var details))
        {
            return false;
        }

        return Contains(details.BusinessPhone, searchText) ||
               Contains(details.Plant, searchText) ||
               Contains(details.Location, searchText) ||
               Contains(details.Team, searchText) ||
               Contains(details.JobFunction, searchText) ||
               Contains(details.PreferredCultureName, searchText) ||
               details.OrganizationUnits.Any(x =>
                   Contains(x.Name, searchText) ||
                   Contains(x.Code, searchText));
    }

    private string GetPrimaryOrganizationGroupLabel(Guid userId)
    {
        if (!_userDetailsCache.TryGetValue(userId, out var details))
        {
            return _noPrimaryOrganizationText;
        }

        var primaryOrganization = details.OrganizationUnits.FirstOrDefault(x => x.IsPrimary);

        if (primaryOrganization is null)
        {
            return _noPrimaryOrganizationText;
        }

        return BuildReferenceLabel(primaryOrganization.Name, primaryOrganization.Code) ?? _noPrimaryOrganizationText;
    }

    private void SyncExpandedGroups()
    {
        var groupKeys = _filteredUsers
            .Select(x => GetPrimaryOrganizationGroupLabel(x.Id))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        _expandedGroups.RemoveWhere(x => !groupKeys.Contains(x));

        foreach (var key in groupKeys)
        {
            _expandedGroups.Add(key);
        }
    }

    private void ToggleGroup(string groupKey)
    {
        if (_expandedGroups.Contains(groupKey))
        {
            _expandedGroups.Remove(groupKey);
            return;
        }

        _expandedGroups.Add(groupKey);
    }

    private static bool Contains(string? value, string searchText) =>
        !string.IsNullOrWhiteSpace(value) &&
        value.Contains(searchText, StringComparison.OrdinalIgnoreCase);

    private sealed record UserGroupViewModel(
        string Key,
        IReadOnlyList<UserListItem> Users);
}