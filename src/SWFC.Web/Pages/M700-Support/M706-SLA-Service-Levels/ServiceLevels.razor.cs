using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Authorization;
using SWFC.Application.M700_Support.M706_SLA_Service_Levels;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Pipeline.Execution;
using SWFC.Web.Components;

namespace SWFC.Web.Pages.M700_Support.M706_SLA_Service_Levels;

public partial class ServiceLevels
{
    [Inject]
    private IExecutionPipeline<GetServiceLevelsQuery, IReadOnlyList<ServiceLevelListItem>> GetServiceLevelsPipeline { get; set; } = default!;

    [Inject]
    private IExecutionPipeline<CreateServiceLevelCommand, Guid> CreateServiceLevelPipeline { get; set; } = default!;

    [Inject]
    private IExecutionPipeline<UpdateServiceLevelCommand, bool> UpdateServiceLevelPipeline { get; set; } = default!;

    private bool _isLoading = true;
    private bool _isSubmitting;
    private bool _showForm;
    private bool _isEdit;
    private string? _error;
    private string? _success;
    private string _searchText = string.Empty;
    private Guid? _selectedId;
    private IReadOnlyList<ServiceLevelListItem> _items = [];
    private List<ServiceLevelListItem> _filteredItems = [];
    private ServiceLevelFormModel _form = new();

    private ServiceLevelListItem? SelectedItem => _items.FirstOrDefault(x => x.Id == _selectedId);

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
            var result = await GetServiceLevelsPipeline.ExecuteAsync(new GetServiceLevelsQuery());

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
        IEnumerable<ServiceLevelListItem> query = _items;

        if (!string.IsNullOrWhiteSpace(_searchText))
        {
            var search = _searchText.Trim();
            query = query.Where(x => x.Priority.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        _filteredItems = query
            .OrderBy(x => x.Priority)
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

        _form = new ServiceLevelFormModel
        {
            Id = SelectedItem.Id,
            Priority = SelectedItem.Priority,
            ResponseMinutes = Math.Max(1, (int)SelectedItem.ResponseTime.TotalMinutes),
            ProcessingMinutes = Math.Max(1, (int)SelectedItem.ProcessingTime.TotalMinutes),
            UseForSupport = SelectedItem.UseForSupport,
            UseForIncidentManagement = SelectedItem.UseForIncidentManagement,
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
            var response = TimeSpan.FromMinutes(_form.ResponseMinutes);
            var processing = TimeSpan.FromMinutes(_form.ProcessingMinutes);

            if (_isEdit)
            {
                var update = await UpdateServiceLevelPipeline.ExecuteAsync(new UpdateServiceLevelCommand(
                    _form.Id,
                    _form.Priority,
                    response,
                    processing,
                    _form.UseForSupport,
                    _form.UseForIncidentManagement,
                    _form.Reason));

                if (!update.IsSuccess)
                {
                    _error = update.Error.Message;
                    return;
                }

                _success = "Service Level aktualisiert.";
                _selectedId = _form.Id;
            }
            else
            {
                var create = await CreateServiceLevelPipeline.ExecuteAsync(new CreateServiceLevelCommand(
                    _form.Priority,
                    response,
                    processing,
                    _form.UseForSupport,
                    _form.UseForIncidentManagement,
                    _form.Reason));

                if (!create.IsSuccess)
                {
                    _error = create.Error.Message;
                    return;
                }

                _success = "Service Level angelegt.";
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

    private static string BuildUsage(ServiceLevelListItem item)
    {
        if (item.UseForSupport && item.UseForIncidentManagement)
        {
            return "Support + Incident";
        }

        return item.UseForSupport ? "Support" : "Incident";
    }

    private static string FormatDuration(TimeSpan value)
    {
        if (value.TotalHours >= 1)
        {
            return $"{value.TotalHours:0.#} h";
        }

        return $"{value.TotalMinutes:0} min";
    }

    private sealed class ServiceLevelFormModel
    {
        public Guid Id { get; set; }
        public string Priority { get; set; } = string.Empty;
        public int ResponseMinutes { get; set; } = 60;
        public int ProcessingMinutes { get; set; } = 240;
        public bool UseForSupport { get; set; } = true;
        public bool UseForIncidentManagement { get; set; } = true;
        public string Reason { get; set; } = string.Empty;
    }
}
