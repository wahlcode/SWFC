namespace SWFC.Web.Pages.M200_Business.M201_Assets.Machines;

public sealed class MachineEditorModel
{
    public string Name { get; set; } = string.Empty;
    public string InventoryNumber { get; set; } = string.Empty;
    public string Status { get; set; } = "Planned";
    public string? Location { get; set; }
    public string? Manufacturer { get; set; }
    public string? MachineModel { get; set; }
    public string? SerialNumber { get; set; }
    public string? Description { get; set; }
    public Guid? ParentMachineId { get; set; }
    public Guid? OrganizationUnitId { get; set; }
    public string Reason { get; set; } = string.Empty;
}
