using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;

namespace SWFC.Domain.M200_Business.M205_Energy.EnergyReadings;

public sealed class EnergyReading
{
    private EnergyReading()
    {
        Id = Guid.Empty;
        Date = null!;
        Value = null!;
        AuditInfo = null!;
    }

    private EnergyReading(
        Guid id,
        Guid meterId,
        EnergyReadingDate date,
        EnergyReadingValue value,
        EnergyReadingSource source,
        string? capturedByUserId,
        EnergyReadingCaptureContext? captureContext,
        EnergyReadingRfidTag? rfidTag,
        EnergyReadingRfidExceptionReason? rfidExceptionReason,
        Guid? offlineCaptureId,
        DateTime? capturedOfflineAtUtc,
        DateTime? syncedAtUtc,
        EnergyReadingPlausibilityStatus plausibilityStatus,
        EnergyReadingPlausibilityNote? plausibilityNote,
        AuditInfo auditInfo)
    {
        Id = id;
        MeterId = meterId;
        Date = date;
        Value = value;
        Source = source;
        CapturedByUserId = capturedByUserId;
        CaptureContext = captureContext;
        RfidTag = rfidTag;
        RfidExceptionReason = rfidExceptionReason;
        OfflineCaptureId = offlineCaptureId;
        CapturedOfflineAtUtc = capturedOfflineAtUtc;
        SyncedAtUtc = syncedAtUtc;
        PlausibilityStatus = plausibilityStatus;
        PlausibilityNote = plausibilityNote;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public Guid MeterId { get; private set; }
    public EnergyReadingDate Date { get; private set; }
    public EnergyReadingValue Value { get; private set; }
    public EnergyReadingSource Source { get; private set; }
    public string? CapturedByUserId { get; private set; }
    public EnergyReadingCaptureContext? CaptureContext { get; private set; }
    public EnergyReadingRfidTag? RfidTag { get; private set; }
    public EnergyReadingRfidExceptionReason? RfidExceptionReason { get; private set; }
    public Guid? OfflineCaptureId { get; private set; }
    public DateTime? CapturedOfflineAtUtc { get; private set; }
    public DateTime? SyncedAtUtc { get; private set; }
    public EnergyReadingPlausibilityStatus PlausibilityStatus { get; private set; }
    public EnergyReadingPlausibilityNote? PlausibilityNote { get; private set; }
    public AuditInfo AuditInfo { get; private set; }

    public static EnergyReading Create(
        Guid meterId,
        EnergyReadingDate date,
        EnergyReadingValue value,
        EnergyReadingSource source,
        string? capturedByUserId,
        EnergyReadingCaptureContext? captureContext,
        EnergyReadingRfidTag? rfidTag,
        EnergyReadingRfidExceptionReason? rfidExceptionReason,
        Guid? offlineCaptureId,
        DateTime? capturedOfflineAtUtc,
        DateTime? syncedAtUtc,
        EnergyReadingPlausibilityStatus plausibilityStatus,
        EnergyReadingPlausibilityNote? plausibilityNote,
        ChangeContext changeContext)
    {
        if (meterId == Guid.Empty)
        {
            throw new ArgumentException("Meter id must not be empty.", nameof(meterId));
        }

        var auditInfo = new AuditInfo(
            changeContext.ChangedAtUtc,
            changeContext.UserId);

        return new EnergyReading(
            Guid.NewGuid(),
            meterId,
            date,
            value,
            source,
            NormalizeOptional(capturedByUserId),
            captureContext,
            rfidTag,
            rfidExceptionReason,
            offlineCaptureId,
            capturedOfflineAtUtc,
            syncedAtUtc,
            plausibilityStatus,
            plausibilityNote,
            auditInfo);
    }

    public void Update(
        EnergyReadingDate date,
        EnergyReadingValue value,
        EnergyReadingSource source,
        string? capturedByUserId,
        EnergyReadingCaptureContext? captureContext,
        EnergyReadingRfidTag? rfidTag,
        EnergyReadingRfidExceptionReason? rfidExceptionReason,
        Guid? offlineCaptureId,
        DateTime? capturedOfflineAtUtc,
        DateTime? syncedAtUtc,
        EnergyReadingPlausibilityStatus plausibilityStatus,
        EnergyReadingPlausibilityNote? plausibilityNote,
        ChangeContext changeContext)
    {
        Date = date;
        Value = value;
        Source = source;
        CapturedByUserId = NormalizeOptional(capturedByUserId);
        CaptureContext = captureContext;
        RfidTag = rfidTag;
        RfidExceptionReason = rfidExceptionReason;
        OfflineCaptureId = offlineCaptureId;
        CapturedOfflineAtUtc = capturedOfflineAtUtc;
        SyncedAtUtc = syncedAtUtc;
        PlausibilityStatus = plausibilityStatus;
        PlausibilityNote = plausibilityNote;

        Touch(changeContext);
    }

    private void Touch(ChangeContext changeContext)
    {
        AuditInfo = new AuditInfo(
            AuditInfo.CreatedAtUtc,
            AuditInfo.CreatedBy,
            changeContext.ChangedAtUtc,
            changeContext.UserId);
    }

    public bool IsPlausibilityFlagged => PlausibilityStatus == EnergyReadingPlausibilityStatus.Flagged;

    public bool IsOfflineCapture => OfflineCaptureId.HasValue || CapturedOfflineAtUtc.HasValue;

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
