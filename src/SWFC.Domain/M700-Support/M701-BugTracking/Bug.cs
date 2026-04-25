using SWFC.Domain.M100_System.M101_Foundation.Exceptions;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;

namespace SWFC.Domain.M700_Support.M701_BugTracking;

public sealed class Bug
{
    private Bug()
    {
        Id = Guid.Empty;
        Description = string.Empty;
        Reproducibility = string.Empty;
        Status = BugStatus.Open;
        AuditInfo = null!;
    }

    private Bug(
        Guid id,
        string description,
        string reproducibility,
        BugStatus status,
        AuditInfo auditInfo)
    {
        Id = id;
        Description = description;
        Reproducibility = reproducibility;
        Status = status;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public string Description { get; private set; }
    public string Reproducibility { get; private set; }
    public BugStatus Status { get; private set; }
    public AuditInfo AuditInfo { get; private set; }

    public static Bug Create(
        string description,
        string reproducibility,
        BugStatus status,
        ChangeContext changeContext)
    {
        ValidateStatus(status);

        var auditInfo = new AuditInfo(
            createdAtUtc: changeContext.ChangedAtUtc,
            createdBy: changeContext.UserId);

        return new Bug(
            Guid.NewGuid(),
            NormalizeRequired(description, nameof(Description)),
            NormalizeRequired(reproducibility, nameof(Reproducibility)),
            status,
            auditInfo);
    }

    public void UpdateDetails(
        string description,
        string reproducibility,
        BugStatus status,
        ChangeContext changeContext)
    {
        ValidateStatus(status);

        Description = NormalizeRequired(description, nameof(Description));
        Reproducibility = NormalizeRequired(reproducibility, nameof(Reproducibility));
        Status = status;

        Touch(changeContext);
    }

    private static string NormalizeRequired(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException($"{fieldName} is required.");
        }

        return value.Trim();
    }

    private static void ValidateStatus(BugStatus status)
    {
        if (!Enum.IsDefined(status))
        {
            throw new ValidationException("Bug status is invalid.");
        }
    }

    private void Touch(ChangeContext changeContext)
    {
        AuditInfo = new AuditInfo(
            createdAtUtc: AuditInfo.CreatedAtUtc,
            createdBy: AuditInfo.CreatedBy,
            lastModifiedAtUtc: changeContext.ChangedAtUtc,
            lastModifiedBy: changeContext.UserId);
    }
}

public enum BugStatus
{
    Open = 0,
    InProgress = 1,
    Resolved = 2
}
