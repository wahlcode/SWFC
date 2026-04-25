using System.ComponentModel.DataAnnotations;
using SWFC.Domain.M200_Business.M202_Maintenance.MaintenanceOrders;
using SWFC.Domain.M200_Business.M202_Maintenance.MaintenancePlans;

namespace SWFC.Web.Pages.M200_Business.M202_Maintenance.Components;

public sealed class MaintenancePlanFormModel
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    public MaintenanceTargetType TargetType { get; set; } = MaintenanceTargetType.Machine;

    [Required]
    public string TargetIdText { get; set; } = string.Empty;

    public int IntervalValue { get; set; } = 1;

    public MaintenancePlanIntervalUnit IntervalUnit { get; set; } = MaintenancePlanIntervalUnit.Months;

    public DateTime NextDueAt { get; set; } = DateTime.Today;

    public bool IsActive { get; set; } = true;
}
