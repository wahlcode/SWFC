using System.ComponentModel.DataAnnotations;
using SWFC.Domain.M200_Business.M205_Energy.EnergyReadings;

namespace SWFC.Web.Pages.M200_Business.M205_Energy.Components;

public sealed class EnergyReadingInputModel
{
    [Required]
    public string MeterId { get; set; } = string.Empty;

    [Required]
    public DateTime Date { get; set; } = DateTime.Today;

    [Range(0, double.MaxValue)]
    public decimal Value { get; set; }

    public EnergyReadingSource Source { get; set; } = EnergyReadingSource.Manual;

    [Required]
    public string Reason { get; set; } = string.Empty;
}
