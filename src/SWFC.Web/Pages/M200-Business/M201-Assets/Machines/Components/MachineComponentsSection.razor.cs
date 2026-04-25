using Microsoft.AspNetCore.Components;
using SWFC.Application.M200_Business.M201_Assets.MachineComponentAreas;
using SWFC.Application.M200_Business.M201_Assets.MachineComponents;
using SWFC.Domain.M800_Security.M801_Access;
using SWFC.Web.Pages.M200_Business.M201_Assets.Machines;
using SWFC.Web.Pages.M200_Business.M201_Assets.Machines.Components.ViewModels;

namespace SWFC.Web.Pages.M200_Business.M201_Assets.Machines.Components;

public partial class MachineComponentsSection
{
    [Parameter]
    public Guid MachineId { get; set; }

    private bool _isLoading = true;
    private bool _isBusy;
    private string? _error;
    private string? _success;

    private IReadOnlyList<MachineComponentAreaVm> _areas = Array.Empty<MachineComponentAreaVm>();
    private IReadOnlyList<MachineComponentVm> _components = Array.Empty<MachineComponentVm>();

    private List<MachineComponentVm> _rootComponents = new();
    private Dictionary<Guid, List<MachineComponentVm>> _childrenByParentId = new();

    private Guid? _selectedAreaId;
    private Guid? _selectedComponentId;
    private MachineComponentEditorModel _editorModel = new();

    private string? _selectedAreaName =>
        _selectedAreaId.HasValue
            ? _areas.FirstOrDefault(x => x.Id == _selectedAreaId.Value)?.Name
            : null;

    private VisibilityTargetVm CurrentVisibilityTarget
    {
        get
        {
            if (_selectedComponentId.HasValue)
            {
                var component = _components.FirstOrDefault(x => x.Id == _selectedComponentId.Value);

                if (component is not null)
                {
                    return new VisibilityTargetVm
                    {
                        TargetType = AccessTargetType.MachineComponent,
                        TargetId = component.Id.ToString(),
                        Title = "Sichtbarkeit der Komponente",
                        Description = component.Name,
                        CanManageRules = true
                    };
                }
            }

            if (_selectedAreaId.HasValue)
            {
                var area = _areas.FirstOrDefault(x => x.Id == _selectedAreaId.Value);

                if (area is not null)
                {
                    return new VisibilityTargetVm
                    {
                        TargetType = AccessTargetType.MachineComponentArea,
                        TargetId = area.Id.ToString(),
                        Title = "Sichtbarkeit des Katalogbereichs",
                        Description = area.Name,
                        CanManageRules = true
                    };
                }
            }

            return new VisibilityTargetVm
            {
                TargetType = AccessTargetType.Machine,
                TargetId = MachineId.ToString(),
                Title = "Sichtbarkeit der Maschine",
                Description = "Regeln gelten für die gesamte Maschine, sofern keine spezielleren Regeln auf Komponente oder Katalogbereich greifen.",
                CanManageRules = MachineId != Guid.Empty
            };
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        await ReloadAllAsync();
    }

    private async Task ReloadAllAsync()
    {
        _isLoading = true;
        _error = null;
        _success = null;

        var areaResult = await GetAreasPipeline.ExecuteAsync(new GetVisibleMachineComponentAreasQuery(MachineId));
        if (areaResult.IsSuccess && areaResult.Value is not null)
        {
            _areas = areaResult.Value
                .Select(Map)
                .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        else
        {
            _error = areaResult.Error?.Message;
        }

        var componentResult = await GetComponentsPipeline.ExecuteAsync(
            new GetVisibleMachineComponentsByMachineQuery(MachineId));

        if (componentResult.IsSuccess && componentResult.Value is not null)
        {
            var mapped = componentResult.Value
                .Select(Map)
                .ToList();

            _components = _selectedAreaId.HasValue
                ? mapped.Where(x => x.AreaId == _selectedAreaId.Value).ToList()
                : mapped;

            BuildTree();
        }
        else
        {
            _error = componentResult.Error?.Message;
        }

        if (_selectedComponentId.HasValue)
        {
            var selected = _components.FirstOrDefault(x => x.Id == _selectedComponentId.Value);

            if (selected is not null)
            {
                LoadEditor(selected);
            }
            else
            {
                _selectedComponentId = null;
                PrepareNewComponent();
            }
        }
        else
        {
            PrepareNewComponent();
        }

        _isLoading = false;
    }

    private void BuildTree()
    {
        _rootComponents = _components
            .Where(x => !x.ParentId.HasValue)
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        _childrenByParentId = _components
            .Where(x => x.ParentId.HasValue)
            .GroupBy(x => x.ParentId!.Value)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToList());
    }

    private void PrepareNewComponent()
    {
        _selectedComponentId = null;
        _editorModel = new MachineComponentEditorModel
        {
            MachineComponentAreaId = _selectedAreaId
        };
    }

    private void LoadEditor(MachineComponentVm component)
    {
        _editorModel = new MachineComponentEditorModel
        {
            Id = component.Id,
            MachineComponentAreaId = component.AreaId,
            ParentMachineComponentId = component.ParentId,
            Name = component.Name,
            Description = component.Description,
            IsActive = component.IsActive
        };
    }

    private async Task HandleSelectAreaAsync(Guid? areaId)
    {
        _selectedAreaId = areaId;
        _selectedComponentId = null;

        if (!_editorModel.Id.HasValue)
        {
            _editorModel.MachineComponentAreaId = areaId;
        }

        await ReloadAllAsync();
    }

    private Task SelectComponentAsync(Guid componentId)
    {
        _selectedComponentId = componentId;

        var component = _components.First(x => x.Id == componentId);
        LoadEditor(component);

        return Task.CompletedTask;
    }

    private async Task SaveComponentAsync()
    {
        _error = null;
        _success = null;
        _isBusy = true;

        try
        {
            if (_editorModel.Id.HasValue)
            {
                var updateResult = await UpdateComponentPipeline.ExecuteAsync(
                    new UpdateMachineComponentCommand(
                        _editorModel.Id.Value,
                        _editorModel.MachineComponentAreaId,
                        _editorModel.Name,
                        _editorModel.Description,
                        _editorModel.Reason));

                if (!updateResult.IsSuccess)
                {
                    _error = updateResult.Error?.Message;
                    return;
                }

                var moveResult = await MoveComponentPipeline.ExecuteAsync(
                    new MoveMachineComponentCommand(
                        _editorModel.Id.Value,
                        MachineId,
                        _editorModel.MachineComponentAreaId,
                        _editorModel.ParentMachineComponentId,
                        _editorModel.Reason));

                if (!moveResult.IsSuccess)
                {
                    _error = moveResult.Error?.Message;
                    return;
                }

                _success = "Komponente aktualisiert.";
            }
            else
            {
                var createResult = await CreateComponentPipeline.ExecuteAsync(
                    new CreateMachineComponentCommand(
                        MachineId,
                        _editorModel.MachineComponentAreaId,
                        _editorModel.ParentMachineComponentId,
                        _editorModel.Name,
                        _editorModel.Description,
                        _editorModel.Reason));

                if (!createResult.IsSuccess)
                {
                    _error = createResult.Error?.Message;
                    return;
                }

                _selectedComponentId = createResult.Value;
                _success = "Komponente erstellt.";
            }

            await ReloadAllAsync();
        }
        finally
        {
            _isBusy = false;
        }
    }

    private async Task ToggleComponentActiveStateAsync()
    {
        if (!_editorModel.Id.HasValue)
        {
            return;
        }

        _error = null;
        _success = null;
        _isBusy = true;

        try
        {
            var result = await SetComponentActiveStatePipeline.ExecuteAsync(
                new SetMachineComponentActiveStateCommand(
                    _editorModel.Id.Value,
                    !_editorModel.IsActive,
                    _editorModel.Reason));

            if (!result.IsSuccess)
            {
                _error = result.Error?.Message;
                return;
            }

            _success = "Komponentenstatus aktualisiert.";
            await ReloadAllAsync();
        }
        finally
        {
            _isBusy = false;
        }
    }

    private Task HandleChildMessageChanged(ComponentMessage message)
    {
        _error = message.IsError ? message.Text : null;
        _success = message.IsError ? null : message.Text;
        return Task.CompletedTask;
    }

    private static MachineComponentVm Map(MachineComponentListItemDto dto)
        => new()
        {
            Id = dto.Id,
            AreaId = dto.MachineComponentAreaId,
            ParentId = dto.ParentMachineComponentId,
            Name = dto.Name,
            Description = dto.Description,
            IsActive = dto.IsActive
        };

    private static MachineComponentAreaVm Map(MachineComponentAreaListItemDto dto)
        => new()
        {
            Id = dto.Id,
            Name = dto.Name,
            IsActive = dto.IsActive
        };

    public sealed record ComponentMessage(bool IsError, string Text);
}
