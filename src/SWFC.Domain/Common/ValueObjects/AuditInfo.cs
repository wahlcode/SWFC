namespace SWFC.Domain.Common.ValueObjects;

public sealed class AuditInfo
{
    public AuditInfo(
        DateTime createdAtUtc,
        string createdBy,
        DateTime? lastModifiedAtUtc = null,
        string? lastModifiedBy = null)
    {
        CreatedAtUtc = createdAtUtc;
        CreatedBy = createdBy;
        LastModifiedAtUtc = lastModifiedAtUtc;
        LastModifiedBy = lastModifiedBy;
    }

    public DateTime CreatedAtUtc { get; }
    public string CreatedBy { get; }
    public DateTime? LastModifiedAtUtc { get; }
    public string? LastModifiedBy { get; }
}