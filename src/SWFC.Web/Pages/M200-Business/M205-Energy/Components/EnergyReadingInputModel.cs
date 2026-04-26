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

    public string? CapturedByUserId { get; set; } = "system";

    public string? CaptureContext { get; set; }

    public string? RfidTag { get; set; }

    public string? RfidExceptionReason { get; set; }

    public string? OfflineCaptureId { get; set; }

    public DateTime? CapturedOfflineAtUtc { get; set; }

    public DateTime? SyncedAtUtc { get; set; }

    public EnergyReadingPlausibilityStatus PlausibilityStatus { get; set; } = EnergyReadingPlausibilityStatus.Normal;

    public string? PlausibilityNote { get; set; }

    [Required]
    public string Reason { get; set; } = string.Empty;
}
