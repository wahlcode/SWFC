using SWFC.Domain.M800_Security.M801_Access;

namespace SWFC.Web.Pages.M200_Business.M201_Assets.Machines.Components.ViewModels;

public sealed class VisibilityTargetVm
{
    public AccessTargetType TargetType { get; init; }
    public string TargetId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool CanManageRules { get; init; }
}
