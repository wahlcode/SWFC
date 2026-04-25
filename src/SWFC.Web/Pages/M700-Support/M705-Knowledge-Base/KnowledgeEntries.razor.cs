using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Authorization;
using SWFC.Application.M700_Support.M705_Knowledge_Base;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Pipeline.Execution;
using SWFC.Domain.M700_Support.M705_Knowledge_Base;
using SWFC.Web.Components;

namespace SWFC.Web.Pages.M700_Support.M705_Knowledge_Base;

public partial class KnowledgeEntries
{
    [Inject]
    private IExecutionPipeline<GetKnowledgeEntriesQuery, IReadOnlyList<KnowledgeEntryListItem>> GetKnowledgeEntriesPipeline { get; set; } = default!;

    [Inject]
    private IExecutionPipeline<CreateKnowledgeEntryCommand, Guid> CreateKnowledgeEntryPipeline { get; set; } = default!;

    [Inject]
    private IExecutionPipeline<UpdateKnowledgeEntryCommand, bool> UpdateKnowledgeEntryPipeline { get; set; } = default!;

    private bool _isLoading = true;
    private bool _isSubmitting;
    private bool _showForm;
    private bool _isEdit;
    private string? _error;
    private string? _success;
    private string _searchText = string.Empty;
    private Guid? _selectedId;
    private IReadOnlyList<KnowledgeEntryListItem> _items = [];
    private List<KnowledgeEntryListItem> _filteredItems = [];
    private KnowledgeEntryFormModel _form = new();

    private KnowledgeEntryListItem? SelectedItem => _items.FirstOrDefault(x => x.Id == _selectedId);

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
            var result = await GetKnowledgeEntriesPipeline.ExecuteAsync(new GetKnowledgeEntriesQuery());

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
        IEnumerable<KnowledgeEntryListItem> query = _items;

        if (!string.IsNullOrWhiteSpace(_searchText))
        {
            var search = _searchText.Trim();
            query = query.Where(x =>
                x.Content.Contains(search, StringComparison.OrdinalIgnoreCase) ||
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

        _form = new KnowledgeEntryFormModel
        {
            Id = SelectedItem.Id,
            Type = SelectedItem.Type,
            Content = SelectedItem.Content,
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
                var update = await UpdateKnowledgeEntryPipeline.ExecuteAsync(new UpdateKnowledgeEntryCommand(
                    _form.Id,
                    _form.Type,
                    _form.Content,
                    _form.Reason));

                if (!update.IsSuccess)
                {
                    _error = update.Error.Message;
                    return;
                }

                _success = "Knowledge-Eintrag aktualisiert.";
                _selectedId = _form.Id;
            }
            else
            {
                var create = await CreateKnowledgeEntryPipeline.ExecuteAsync(new CreateKnowledgeEntryCommand(
                    _form.Type,
                    _form.Content,
                    _form.Reason));

                if (!create.IsSuccess)
                {
                    _error = create.Error.Message;
                    return;
                }

                _success = "Knowledge-Eintrag angelegt.";
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

    private static string FormatType(KnowledgeEntryType type)
    {
        return type switch
        {
            KnowledgeEntryType.ProblemSolution => "Problemloesung",
            KnowledgeEntryType.Instruction => "Anleitung",
            KnowledgeEntryType.BestPractice => "Best Practice",
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

    private sealed class KnowledgeEntryFormModel
    {
        public Guid Id { get; set; }
        public KnowledgeEntryType Type { get; set; } = KnowledgeEntryType.ProblemSolution;
        public string Content { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}
