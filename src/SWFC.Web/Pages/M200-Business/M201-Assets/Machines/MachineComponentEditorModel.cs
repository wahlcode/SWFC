namespace SWFC.Web.Pages.M200_Business.M201_Assets.Machines;

public sealed class MachineComponentEditorModel
{
    public Guid? Id { get; set; }
    public Guid? MachineComponentAreaId { get; set; }
    public Guid? ParentMachineComponentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Reason { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
