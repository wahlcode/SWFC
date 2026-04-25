using SWFC.Web.Components.Layout;

namespace SWFC.Web.Pages.M300_Presentation.M306_Localization.Services;

public sealed class NavigationModuleLabelService
{
    private readonly SidebarConfigService _sidebarConfigService;
    private readonly LocalizationTextProvider _textProvider;

    public NavigationModuleLabelService(
        SidebarConfigService sidebarConfigService,
        LocalizationTextProvider textProvider)
    {
        _sidebarConfigService = sidebarConfigService;
        _textProvider = textProvider;
    }

    public async Task<IReadOnlyList<NavigationModuleLabel>> GetNavigationModuleLabelsAsync(
        string cultureName,
        CancellationToken cancellationToken = default)
    {
        var groups = await _sidebarConfigService.GetGroupsAsync(cancellationToken);
        var labels = new List<NavigationModuleLabel>();

        foreach (var group in groups)
        {
            var groupLabel = await _textProvider.GetTextAsync(
                group.DisplayNameKey,
                cultureName,
                cancellationToken);

            foreach (var module in group.Modules)
            {
                labels.Add(new NavigationModuleLabel(
                    group.Key,
                    groupLabel,
                    module.Code,
                    await _textProvider.GetTextAsync(module.DisplayNameKey, cultureName, cancellationToken),
                    module.Route,
                    module.IsActive));
            }
        }

        return labels;
    }
}

public sealed record NavigationModuleLabel(
    string GroupKey,
    string GroupLabel,
    string ModuleCode,
    string ModuleLabel,
    string Route,
    bool IsActive);
