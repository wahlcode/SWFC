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
        AuditInfo auditInfo)
    {
        Id = id;
        MeterId = meterId;
        Date = date;
        Value = value;
        Source = source;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public Guid MeterId { get; private set; }
    public EnergyReadingDate Date { get; private set; }
    public EnergyReadingValue Value { get; private set; }
    public EnergyReadingSource Source { get; private set; }
    public AuditInfo AuditInfo { get; private set; }

    public static EnergyReading Create(
        Guid meterId,
        EnergyReadingDate date,
        EnergyReadingValue value,
        EnergyReadingSource source,
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
            auditInfo);
    }

    public void Update(
        EnergyReadingDate date,
        EnergyReadingValue value,
        EnergyReadingSource source,
        ChangeContext changeContext)
    {
        Date = date;
        Value = value;
        Source = source;

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
}
