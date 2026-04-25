using SWFC.Domain.M100_System.M101_Foundation.Rules;

namespace SWFC.Domain.M100_System.M101_Foundation.ValueObjects;

public sealed class AuditInfo
{
    public AuditInfo(
        DateTime createdAtUtc,
        string createdBy,
        DateTime? lastModifiedAtUtc = null,
        string? lastModifiedBy = null)
    {
        Guard.AgainstNullOrWhiteSpace(createdBy, nameof(createdBy));

        CreatedAtUtc = createdAtUtc;
        CreatedBy = createdBy.Trim();
        LastModifiedAtUtc = lastModifiedAtUtc;
        LastModifiedBy = string.IsNullOrWhiteSpace(lastModifiedBy) ? null : lastModifiedBy.Trim();
    }

    public DateTime CreatedAtUtc { get; }
    public string CreatedBy { get; }
    public DateTime? LastModifiedAtUtc { get; }
    public string? LastModifiedBy { get; }
}

