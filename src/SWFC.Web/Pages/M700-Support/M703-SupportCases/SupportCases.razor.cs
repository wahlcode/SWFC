using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Authorization;
using SWFC.Application.M700_Support.M703_SupportCases;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Pipeline.Execution;
using SWFC.Domain.M700_Support.M704_Incident_Management;
using SWFC.Web.Components;

namespace SWFC.Web.Pages.M700_Support.M703_SupportCases;

public partial class SupportCases
{
    [Inject]
    private IExecutionPipeline<GetSupportCasesQuery, IReadOnlyList<SupportCaseListItem>> GetSupportCasesPipeline { get; set; } = default!;

    [Inject]
    private IExecutionPipeline<CreateSupportCaseCommand, Guid> CreateSupportCasePipeline { get; set; } = default!;

    [Inject]
    private IExecutionPipeline<UpdateSupportCaseCommand, bool> UpdateSupportCasePipeline { get; set; } = default!;

    [Inject]
    private IExecutionPipeline<EscalateSupportCaseToBugCommand, Guid> EscalateToBugPipeline { get; set; } = default!;

    [Inject]
    private IExecutionPipeline<EscalateSupportCaseToIncidentCommand, Guid> EscalateToIncidentPipeline { get; set; } = default!;

    private bool _isLoading = true;
    private bool _isSubmitting;
    private bool _showCaseForm;
    private bool _showBugEscalation;
    private bool _showIncidentEscalation;
    private bool _isEdit;
    private string? _error;
    private string? _success;
    private string _searchText = string.Empty;
    private Guid? _selectedId;
    private IReadOnlyList<SupportCaseListItem> _items = [];
    private List<SupportCaseListItem> _filteredItems = [];
    private SupportCaseFormModel _caseForm = new();
    private BugEscalationModel _bugEscalation = new();
    private IncidentEscalationModel _incidentEscalation = new();

    private SupportCaseListItem? SelectedItem => _items.FirstOrDefault(x => x.Id == _selectedId);

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
            var result = await GetSupportCasesPipeline.ExecuteAsync(new GetSupportCasesQuery());

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
        IEnumerable<SupportCaseListItem> query = _items;

        if (!string.IsNullOrWhiteSpace(_searchText))
        {
            var search = _searchText.Trim();
            query = query.Where(x =>
                x.UserRequest.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                x.ProblemDescription.Contains(search, StringComparison.OrdinalIgnoreCase));
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
        _caseForm = new();
        _isEdit = false;
        ShowOnlyCaseForm();
    }

    private void OpenEditForm()
    {
        if (SelectedItem is null)
        {
            return;
        }

        _caseForm = new SupportCaseFormModel
        {
            Id = SelectedItem.Id,
            UserRequest = SelectedItem.UserRequest,
            ProblemDescription = SelectedItem.ProblemDescription,
            Reason = string.Empty
        };
        _isEdit = true;
        ShowOnlyCaseForm();
    }

    private void OpenBugEscalation()
    {
        if (SelectedItem is null)
        {
            return;
        }

        _bugEscalation = new();
        _showCaseForm = false;
        _showIncidentEscalation = false;
        _showBugEscalation = true;
        _error = null;
        _success = null;
    }

    private void OpenIncidentEscalation()
    {
        if (SelectedItem is null)
        {
            return;
        }

        _incidentEscalation = new();
        _showCaseForm = false;
        _showBugEscalation = false;
        _showIncidentEscalation = true;
        _error = null;
        _success = null;
    }

    private void ShowOnlyCaseForm()
    {
        _showCaseForm = true;
        _showBugEscalation = false;
        _showIncidentEscalation = false;
        _error = null;
        _success = null;
    }

    private void CloseForms()
    {
        if (_isSubmitting)
        {
            return;
        }

        _showCaseForm = false;
        _showBugEscalation = false;
        _showIncidentEscalation = false;
    }

    private async Task SaveCaseAsync()
    {
        _isSubmitting = true;
        _error = null;
        _success = null;

        try
        {
            if (_isEdit)
            {
                var update = await UpdateSupportCasePipeline.ExecuteAsync(new UpdateSupportCaseCommand(
                    _caseForm.Id,
                    _caseForm.UserRequest,
                    _caseForm.ProblemDescription,
                    _caseForm.Reason));

                if (!update.IsSuccess)
                {
                    _error = update.Error.Message;
                    return;
                }

                _success = "Supportfall aktualisiert.";
                _selectedId = _caseForm.Id;
            }
            else
            {
                var create = await CreateSupportCasePipeline.ExecuteAsync(new CreateSupportCaseCommand(
                    _caseForm.UserRequest,
                    _caseForm.ProblemDescription,
                    _caseForm.Reason));

                if (!create.IsSuccess)
                {
                    _error = create.Error.Message;
                    return;
                }

                _success = "Supportfall angelegt.";
                _selectedId = create.Value;
            }

            CloseForms();
            await LoadAsync();
        }
        finally
        {
            _isSubmitting = false;
        }
    }

    private async Task EscalateBugAsync()
    {
        if (SelectedItem is null)
        {
            return;
        }

        _isSubmitting = true;
        _error = null;
        _success = null;

        try
        {
            var result = await EscalateToBugPipeline.ExecuteAsync(new EscalateSupportCaseToBugCommand(
                SelectedItem.Id,
                _bugEscalation.Reproducibility,
                _bugEscalation.Reason));

            if (!result.IsSuccess)
            {
                _error = result.Error.Message;
                return;
            }

            _success = $"Bug {result.Value} erzeugt und verlinkt.";
            CloseForms();
            await LoadAsync();
        }
        finally
        {
            _isSubmitting = false;
        }
    }

    private async Task EscalateIncidentAsync()
    {
        if (SelectedItem is null)
        {
            return;
        }

        _isSubmitting = true;
        _error = null;
        _success = null;

        try
        {
            var result = await EscalateToIncidentPipeline.ExecuteAsync(new EscalateSupportCaseToIncidentCommand(
                SelectedItem.Id,
                _incidentEscalation.Category,
                _incidentEscalation.Escalation,
                _incidentEscalation.ReactionControl,
                _incidentEscalation.NotificationReference,
                _incidentEscalation.RuntimeReference,
                _incidentEscalation.Reason));

            if (!result.IsSuccess)
            {
                _error = result.Error.Message;
                return;
            }

            _success = $"Incident {result.Value} erzeugt und verlinkt.";
            CloseForms();
            await LoadAsync();
        }
        finally
        {
            _isSubmitting = false;
        }
    }

    private static string BuildEscalationState(SupportCaseListItem item)
    {
        if (item.TriggeredBugId.HasValue && item.TriggeredIncidentId.HasValue)
        {
            return "Bug + Incident";
        }

        if (item.TriggeredBugId.HasValue)
        {
            return "Bug";
        }

        if (item.TriggeredIncidentId.HasValue)
        {
            return "Incident";
        }

        return "Support";
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

    private sealed class SupportCaseFormModel
    {
        public Guid Id { get; set; }
        public string UserRequest { get; set; } = string.Empty;
        public string ProblemDescription { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    private sealed class BugEscalationModel
    {
        public string Reproducibility { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    private sealed class IncidentEscalationModel
    {
        public IncidentCategory Category { get; set; } = IncidentCategory.SystemOutage;
        public string Escalation { get; set; } = string.Empty;
        public string ReactionControl { get; set; } = string.Empty;
        public string NotificationReference { get; set; } = string.Empty;
        public string RuntimeReference { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}
