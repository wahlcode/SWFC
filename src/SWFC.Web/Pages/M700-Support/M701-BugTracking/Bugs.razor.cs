using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Authorization;
using SWFC.Application.M700_Support.M701_BugTracking;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Pipeline.Execution;
using SWFC.Domain.M700_Support.M701_BugTracking;
using SWFC.Web.Components;

namespace SWFC.Web.Pages.M700_Support.M701_BugTracking;

public partial class Bugs
{
    [Inject]
    private IExecutionPipeline<GetBugsQuery, IReadOnlyList<BugListItem>> GetBugsPipeline { get; set; } = default!;

    [Inject]
    private IExecutionPipeline<CreateBugCommand, Guid> CreateBugPipeline { get; set; } = default!;

    [Inject]
    private IExecutionPipeline<UpdateBugCommand, bool> UpdateBugPipeline { get; set; } = default!;

    private bool _isLoading = true;
    private bool _isSubmitting;
    private bool _showForm;
    private bool _isEdit;
    private string? _error;
    private string? _success;
    private string _searchText = string.Empty;
    private Guid? _selectedId;
    private IReadOnlyList<BugListItem> _items = [];
    private List<BugListItem> _filteredItems = [];
    private BugFormModel _form = new();

    private BugListItem? SelectedItem => _items.FirstOrDefault(x => x.Id == _selectedId);

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        _isLoading = true;
        _error = null;

        try
        {
            var result = await GetBugsPipeline.ExecuteAsync(new GetBugsQuery());

            if (!result.IsSuccess || result.Value is null)
            {
                _error = result.Error.Message;
                return;
            }

            _items = result.Value;
            ApplyFilter();
            _selectedId ??= _filteredItems.FirstOrDefault()?.Id;
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void ApplyFilter()
    {
        IEnumerable<BugListItem> query = _items;

        if (!string.IsNullOrWhiteSpace(_searchText))
        {
            var search = _searchText.Trim();
            query = query.Where(x =>
                x.Description.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                x.Reproducibility.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                x.Status.ToString().Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        _filteredItems = query
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToList();
    }

    private void OnSearchChanged(ChangeEventArgs e)
    {
        _searchText = e.Value?.ToString() ?? string.Empty;
        ApplyFilter();
    }

    private void SelectItem(Guid id)
    {
        _selectedId = id;
        _success = null;
    }

    private void OpenCreateForm()
    {
        _form = new();
        _isEdit = false;
        _showForm = true;
        _error = null;
        _success = null;
    }

    private void OpenEditForm()
    {
        if (SelectedItem is null)
        {
            return;
        }

        _form = new BugFormModel
        {
            Id = SelectedItem.Id,
            Description = SelectedItem.Description,
            Reproducibility = SelectedItem.Reproducibility,
            Status = SelectedItem.Status,
            Reason = string.Empty
        };
        _isEdit = true;
        _showForm = true;
        _error = null;
        _success = null;
    }

    private void CloseForm()
    {
        if (!_isSubmitting)
        {
            _showForm = false;
        }
    }

    private async Task SaveAsync()
    {
        _isSubmitting = true;
        _error = null;
        _success = null;

        try
        {
            if (_isEdit)
            {
                var update = await UpdateBugPipeline.ExecuteAsync(new UpdateBugCommand(
                    _form.Id,
                    _form.Description,
                    _form.Reproducibility,
                    _form.Status,
                    _form.Reason));

                if (!update.IsSuccess)
                {
                    _error = update.Error.Message;
                    return;
                }

                _success = "Bug aktualisiert.";
                _selectedId = _form.Id;
            }
            else
            {
                var create = await CreateBugPipeline.ExecuteAsync(new CreateBugCommand(
                    _form.Description,
                    _form.Reproducibility,
                    _form.Status,
                    _form.Reason));

                if (!create.IsSuccess)
                {
                    _error = create.Error.Message;
                    return;
                }

                _success = "Bug angelegt.";
                _selectedId = create.Value;
            }

            _showForm = false;
            await LoadAsync();
        }
        finally
        {
            _isSubmitting = false;
        }
    }

    private static string FormatStatus(BugStatus status)
    {
        return status switch
        {
            BugStatus.Open => "Offen",
            BugStatus.InProgress => "In Arbeit",
            BugStatus.Resolved => "Geloest",
            _ => status.ToString()
        };
    }

    private static string FormatDate(DateTime value)
    {
        return value.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
    }

    private static string Preview(string value, int length = 80)
    {
        if (value.Length <= length)
        {
            return value;
        }

        return string.Concat(value.AsSpan(0, length), "...");
    }

    private sealed class BugFormModel
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Reproducibility { get; set; } = string.Empty;
        public BugStatus Status { get; set; } = BugStatus.Open;
        public string Reason { get; set; } = string.Empty;
    }
}
