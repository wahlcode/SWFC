namespace SWFC.Web.Pages.M200_Business.M201_Assets.Machines.Components.ViewModels;

public sealed class MachineComponentVm
{
    public Guid Id { get; init; }
    public Guid? AreaId { get; init; }
    public Guid? ParentId { get; init; }

    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;

    public bool IsActive { get; init; }
}
