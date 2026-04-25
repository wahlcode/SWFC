using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Authorization;
using SWFC.Application.M700_Support.M701_BugTracking;
using SWFC.Application.M700_Support.M702_ChangeRequests;
using SWFC.Application.M700_Support.M703_SupportCases;
using SWFC.Application.M700_Support.M704_Incident_Management;
using SWFC.Application.M700_Support.M705_Knowledge_Base;
using SWFC.Application.M700_Support.M706_SLA_Service_Levels;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Pipeline.Execution;
using SWFC.Domain.M700_Support.M701_BugTracking;
using SWFC.Web.Components;

namespace SWFC.Web.Pages.M700_Support;

public partial class Overview
{
    [Inject]
    private IExecutionPipeline<GetBugsQuery, IReadOnlyList<BugListItem>> GetBugsPipeline { get; set; } = default!;

    [Inject]
    private IExecutionPipeline<GetChangeRequestsQuery, IReadOnlyList<ChangeRequestListItem>> GetChangeRequestsPipeline { get; set; } = default!;

    [Inject]
    private IExecutionPipeline<GetSupportCasesQuery, IReadOnlyList<SupportCaseListItem>> GetSupportCasesPipeline { get; set; } = default!;

    [Inject]
    private IExecutionPipeline<GetIncidentsQuery, IReadOnlyList<IncidentListItem>> GetIncidentsPipeline { get; set; } = default!;

    [Inject]
    private IExecutionPipeline<GetKnowledgeEntriesQuery, IReadOnlyList<KnowledgeEntryListItem>> GetKnowledgeEntriesPipeline { get; set; } = default!;

    [Inject]
    private IExecutionPipeline<GetServiceLevelsQuery, IReadOnlyList<ServiceLevelListItem>> GetServiceLevelsPipeline { get; set; } = default!;

    private bool _isLoading = true;
    private string? _error;

    private IReadOnlyList<BugListItem> _bugs = [];
    private IReadOnlyList<ChangeRequestListItem> _changeRequests = [];
    private IReadOnlyList<SupportCaseListItem> _supportCases = [];
    private IReadOnlyList<IncidentListItem> _incidents = [];
    private IReadOnlyList<KnowledgeEntryListItem> _knowledgeEntries = [];
    private IReadOnlyList<ServiceLevelListItem> _serviceLevels = [];

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
            var bugs = await GetBugsPipeline.ExecuteAsync(new GetBugsQuery());
            var changes = await GetChangeRequestsPipeline.ExecuteAsync(new GetChangeRequestsQuery());
            var cases = await GetSupportCasesPipeline.ExecuteAsync(new GetSupportCasesQuery());
            var incidents = await GetIncidentsPipeline.ExecuteAsync(new GetIncidentsQuery());
            var knowledge = await GetKnowledgeEntriesPipeline.ExecuteAsync(new GetKnowledgeEntriesQuery());
            var levels = await GetServiceLevelsPipeline.ExecuteAsync(new GetServiceLevelsQuery());

            if (!bugs.IsSuccess || !changes.IsSuccess || !cases.IsSuccess || !incidents.IsSuccess || !knowledge.IsSuccess || !levels.IsSuccess)
            {
                _error = "M700-Daten konnten nicht vollstaendig geladen werden.";
                return;
            }

            _bugs = bugs.Value ?? [];
            _changeRequests = changes.Value ?? [];
            _supportCases = cases.Value ?? [];
            _incidents = incidents.Value ?? [];
            _knowledgeEntries = knowledge.Value ?? [];
            _serviceLevels = levels.Value ?? [];
        }
        finally
        {
            _isLoading = false;
        }
    }
}
