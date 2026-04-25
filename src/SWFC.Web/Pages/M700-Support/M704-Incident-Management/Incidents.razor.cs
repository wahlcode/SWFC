using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Authorization;
using SWFC.Application.M700_Support.M704_Incident_Management;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Pipeline.Execution;
using SWFC.Domain.M700_Support.M704_Incident_Management;
using SWFC.Web.Components;

namespace SWFC.Web.Pages.M700_Support.M704_Incident_Management;

public partial class Incidents
{
    [Inject]
    private IExecutionPipeline<GetIncidentsQuery, IReadOnlyList<IncidentListItem>> GetIncidentsPipeline { get; set; } = default!;

    [Inject]
    private IExecutionPipeline<CreateIncidentCommand, Guid> CreateIncidentPipeline { get; set; } = default!;

    [Inject]
    private IExecutionPipeline<UpdateIncidentCommand, bool> UpdateIncidentPipeline { get; set; } = default!;

    private bool _isLoading = true;
    private bool _isSubmitting;
    private bool _showForm;
    private bool _isEdit;
    private string? _error;
    private string? _success;
    private string _searchText = string.Empty;
    private Guid? _selectedId;
    private IReadOnlyList<IncidentListItem> _items = [];
    private List<IncidentListItem> _filteredItems = [];
    private IncidentFormModel _form = new();

    private IncidentListItem? SelectedItem => _items.FirstOrDefault(x => x.Id == _selectedId);

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
            var result = await GetIncidentsPipeline.ExecuteAsync(new GetIncidentsQuery());

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
        IEnumerable<IncidentListItem> query = _items;

        if (!string.IsNullOrWhiteSpace(_searchText))
        {
            var search = _searchText.Trim();
            query = query.Where(x =>
                x.Description.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                x.Escalation.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                x.ReactionControl.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (x.NotificationReference?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (x.RuntimeReference?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                x.Category.ToString().Contains(search, StringComparison.OrdinalIgnoreCase));
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

        _form = new IncidentFormModel
        {
            Id = SelectedItem.Id,
            Category = SelectedItem.Category,
            Description = SelectedItem.Description,
            Escalation = SelectedItem.Escalation,
            ReactionControl = SelectedItem.ReactionControl,
            NotificationReference = SelectedItem.NotificationReference ?? string.Empty,
            RuntimeReference = SelectedItem.RuntimeReference ?? string.Empty,
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
                var update = await UpdateIncidentPipeline.ExecuteAsync(new UpdateIncidentCommand(
                    _form.Id,
                    _form.Category,
                    _form.Description,
                    _form.Escalation,
                    _form.ReactionControl,
                    _form.NotificationReference,
                    _form.RuntimeReference,
                    _form.Reason));

                if (!update.IsSuccess)
                {
                    _error = update.Error.Message;
                    return;
                }

                _success = "Incident aktualisiert.";
                _selectedId = _form.Id;
            }
            else
            {
                var create = await CreateIncidentPipeline.ExecuteAsync(new CreateIncidentCommand(
                    _form.Category,
                    _form.Description,
                    _form.Escalation,
                    _form.ReactionControl,
                    _form.NotificationReference,
                    _form.RuntimeReference,
                    _form.Reason));

                if (!create.IsSuccess)
                {
                    _error = create.Error.Message;
                    return;
                }

                _success = "Incident angelegt.";
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

    private static string BuildReferenceLine(IncidentListItem item)
    {
        var notification = string.IsNullOrWhiteSpace(item.NotificationReference) ? "M303 -" : $"M303 {item.NotificationReference}";
        var runtime = string.IsNullOrWhiteSpace(item.RuntimeReference) ? "M500 -" : $"M500 {item.RuntimeReference}";
        return $"{notification} · {runtime}";
    }

    private static string FormatCategory(IncidentCategory category)
    {
        return category switch
        {
            IncidentCategory.PlantShutdown => "Anlagenstillstand",
            IncidentCategory.SystemOutage => "Systemausfall",
            IncidentCategory.SecurityIncident => "Security Incident",
            _ => category.ToString()
        };
    }

    private static string Preview(string value, int length = 80)
    {
        return value.Length <= length ? value : value[..length] + "...";
    }

    private sealed class IncidentFormModel
    {
        public Guid Id { get; set; }
        public IncidentCategory Category { get; set; } = IncidentCategory.SystemOutage;
        public string Description { get; set; } = string.Empty;
        public string Escalation { get; set; } = string.Empty;
        public string ReactionControl { get; set; } = string.Empty;
        public string NotificationReference { get; set; } = string.Empty;
        public string RuntimeReference { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}
