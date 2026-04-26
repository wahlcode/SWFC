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
        ModuleReference = null;
        ObjectReference = null;
        HistoryLog = string.Empty;
        AuditInfo = null!;
    }

    private Bug(
        Guid id,
        string description,
        string reproducibility,
        BugStatus status,
        string? moduleReference,
        string? objectReference,
        string historyLog,
        AuditInfo auditInfo)
    {
        Id = id;
        Description = description;
        Reproducibility = reproducibility;
        Status = status;
        ModuleReference = moduleReference;
        ObjectReference = objectReference;
        HistoryLog = historyLog;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public string Description { get; private set; }
    public string Reproducibility { get; private set; }
    public BugStatus Status { get; private set; }
    public string? ModuleReference { get; private set; }
    public string? ObjectReference { get; private set; }
    public string HistoryLog { get; private set; }
    public AuditInfo AuditInfo { get; private set; }

    public static Bug Create(
        string description,
        string reproducibility,
        BugStatus status,
        ChangeContext changeContext,
        string? moduleReference = null,
        string? objectReference = null)
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
            NormalizeOptional(moduleReference),
            NormalizeOptional(objectReference),
            CreateHistory("Created", status.ToString(), changeContext),
            auditInfo);
    }

    public void UpdateDetails(
        string description,
        string reproducibility,
        BugStatus status,
        ChangeContext changeContext,
        string? moduleReference = null,
        string? objectReference = null)
    {
        ValidateStatus(status);

        Description = NormalizeRequired(description, nameof(Description));
        Reproducibility = NormalizeRequired(reproducibility, nameof(Reproducibility));
        Status = status;
        ModuleReference = NormalizeOptional(moduleReference) ?? ModuleReference;
        ObjectReference = NormalizeOptional(objectReference) ?? ObjectReference;
        HistoryLog = AppendHistory(HistoryLog, "Updated", status.ToString(), changeContext);

        Touch(changeContext);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string CreateHistory(string action, string value, ChangeContext changeContext) =>
        $"{changeContext.ChangedAtUtc:O}|{changeContext.UserId}|{action}|{value}|{changeContext.Reason}";

    private static string AppendHistory(string historyLog, string action, string value, ChangeContext changeContext) =>
        string.IsNullOrWhiteSpace(historyLog)
            ? CreateHistory(action, value, changeContext)
            : $"{historyLog}{Environment.NewLine}{CreateHistory(action, value, changeContext)}";

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
