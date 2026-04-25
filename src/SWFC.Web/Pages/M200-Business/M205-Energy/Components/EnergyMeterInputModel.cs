using System.ComponentModel.DataAnnotations;
using SWFC.Domain.M200_Business.M205_Energy.EnergyMeters;

namespace SWFC.Web.Pages.M200_Business.M205_Energy.Components;

public sealed class EnergyMeterInputModel
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public EnergyMediumType MediumType { get; set; } = EnergyMediumType.Electricity;

    [Required]
    public string Unit { get; set; } = string.Empty;

    public bool IsManualEntryEnabled { get; set; } = true;

    public bool IsExternalImportEnabled { get; set; }

    public string? ExternalSystem { get; set; }

    public string? RfidTag { get; set; }

    public bool SupportsOfflineCapture { get; set; }

    public string? MachineId { get; set; }

    public bool IsActive { get; set; } = true;

    [Required]
    public string Reason { get; set; } = string.Empty;
}
