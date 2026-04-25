using System.ComponentModel.DataAnnotations;
using SWFC.Domain.M200_Business.M202_Maintenance.MaintenanceOrders;

namespace SWFC.Web.Pages.M200_Business.M202_Maintenance.Components;

public sealed class MaintenanceOrderFormModel
{
    [Required]
    public string Number { get; set; } = string.Empty;

    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    public MaintenanceOrderType Type { get; set; } = MaintenanceOrderType.Planned;

    public MaintenanceTargetType TargetType { get; set; } = MaintenanceTargetType.Machine;

    [Required]
    public string TargetIdText { get; set; } = string.Empty;

    public string MaterialItemId { get; set; } = string.Empty;

    public int MaterialQuantity { get; set; } = 1;
}
