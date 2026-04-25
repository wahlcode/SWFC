using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Authorization;
using SWFC.Application.M700_Support.M702_ChangeRequests;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Pipeline.Execution;
using SWFC.Domain.M700_Support.M702_ChangeRequests;
using SWFC.Web.Components;

namespace SWFC.Web.Pages.M700_Support.M702_ChangeRequests;

public partial class ChangeRequests
{
    [Inject]
    private IExecutionPipeline<GetChangeRequestsQuery, IReadOnlyList<ChangeRequestListItem>> GetChangeRequestsPipeline { get; set; } = default!;

    [Inject]
    private IExecutionPipeline<CreateChangeRequestCommand, Guid> CreateChangeRequestPipeline { get; set; } = default!;

    [Inject]
    private IExecutionPipeline<UpdateChangeRequestCommand, bool> UpdateChangeRequestPipeline { get; set; } = default!;

    private bool _isLoading = true;
    private bool _isSubmitting;
    private bool _showForm;
    private bool _isEdit;
    private string? _error;
    private string? _success;
    private string _searchText = string.Empty;
    private Guid? _selectedId;
    private IReadOnlyList<ChangeRequestListItem> _items = [];
    private List<ChangeRequestListItem> _filteredItems = [];
    private ChangeRequestFormModel _form = new();

    private ChangeRequestListItem? SelectedItem => _items.FirstOrDefault(x => x.Id == _selectedId);

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
            var result = await GetChangeRequestsPipeline.ExecuteAsync(new GetChangeRequestsQuery());

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
        IEnumerable<ChangeRequestListItem> query = _items;

        if (!string.IsNullOrWhiteSpace(_searchText))
        {
            var search = _searchText.Trim();
            query = query.Where(x =>
                x.Description.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (x.RequirementReference?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (x.RoadmapReference?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                x.Type.ToString().Contains(search, StringComparison.OrdinalIgnoreCase));
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

        _form = new ChangeRequestFormModel
        {
            Id = SelectedItem.Id,
            Type = SelectedItem.Type,
            Description = SelectedItem.Description,
            RequirementReference = SelectedItem.RequirementReference ?? string.Empty,
            RoadmapReference = SelectedItem.RoadmapReference ?? string.Empty,
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
                var update = await UpdateChangeRequestPipeline.ExecuteAsync(new UpdateChangeRequestCommand(
                    _form.Id,
                    _form.Type,
                    _form.Description,
                    _form.RequirementReference,
                    _form.RoadmapReference,
                    _form.Reason));

                if (!update.IsSuccess)
                {
                    _error = update.Error.Message;
                    return;
                }

                _success = "Change Request aktualisiert.";
                _selectedId = _form.Id;
            }
            else
            {
                var create = await CreateChangeRequestPipeline.ExecuteAsync(new CreateChangeRequestCommand(
                    _form.Type,
                    _form.Description,
                    _form.RequirementReference,
                    _form.RoadmapReference,
                    _form.Reason));

                if (!create.IsSuccess)
                {
                    _error = create.Error.Message;
                    return;
                }

                _success = "Change Request angelegt.";
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

    private static string BuildReferenceLine(ChangeRequestListItem item)
    {
        var requirement = string.IsNullOrWhiteSpace(item.RequirementReference) ? "M606 -" : $"M606 {item.RequirementReference}";
        var roadmap = string.IsNullOrWhiteSpace(item.RoadmapReference) ? "M601 -" : $"M601 {item.RoadmapReference}";
        return $"{requirement} · {roadmap}";
    }

    private static string FormatType(ChangeRequestType type)
    {
        return type switch
        {
            ChangeRequestType.ChangeRequest => "Change Request",
            ChangeRequestType.ImprovementIdea => "Verbesserungsidee",
            ChangeRequestType.Extension => "Erweiterung",
            _ => type.ToString()
        };
    }

    private static string FormatDate(DateTime value)
    {
        return value.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
    }

    private static string Preview(string value, int length = 80)
    {
        return value.Length <= length ? value : value[..length] + "...";
    }

    private sealed class ChangeRequestFormModel
    {
        public Guid Id { get; set; }
        public ChangeRequestType Type { get; set; } = ChangeRequestType.ChangeRequest;
        public string Description { get; set; } = string.Empty;
        public string RequirementReference { get; set; } = string.Empty;
        public string RoadmapReference { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}
