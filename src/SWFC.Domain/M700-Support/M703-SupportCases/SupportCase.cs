using SWFC.Domain.M100_System.M101_Foundation.Exceptions;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;

namespace SWFC.Domain.M700_Support.M703_SupportCases;

public sealed class SupportCase
{
    private SupportCase()
    {
        Id = Guid.Empty;
        UserRequest = string.Empty;
        ProblemDescription = string.Empty;
        Status = SupportCaseStatus.Open;
        ModuleReference = null;
        ObjectReference = null;
        HistoryLog = string.Empty;
        TriggeredBugId = null;
        TriggeredIncidentId = null;
        AuditInfo = null!;
    }

    private SupportCase(
        Guid id,
        string userRequest,
        string problemDescription,
        SupportCaseStatus status,
        string? moduleReference,
        string? objectReference,
        string historyLog,
        Guid? triggeredBugId,
        Guid? triggeredIncidentId,
        AuditInfo auditInfo)
    {
        Id = id;
        UserRequest = userRequest;
        ProblemDescription = problemDescription;
        Status = status;
        ModuleReference = moduleReference;
        ObjectReference = objectReference;
        HistoryLog = historyLog;
        TriggeredBugId = triggeredBugId;
        TriggeredIncidentId = triggeredIncidentId;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public string UserRequest { get; private set; }
    public string ProblemDescription { get; private set; }
    public SupportCaseStatus Status { get; private set; }
    public string? ModuleReference { get; private set; }
    public string? ObjectReference { get; private set; }
    public string HistoryLog { get; private set; }
    public Guid? TriggeredBugId { get; private set; }
    public Guid? TriggeredIncidentId { get; private set; }
    public AuditInfo AuditInfo { get; private set; }

    public static SupportCase Create(
        string userRequest,
        string problemDescription,
        ChangeContext changeContext,
        SupportCaseStatus status = SupportCaseStatus.Open,
        string? moduleReference = null,
        string? objectReference = null)
    {
        ValidateStatus(status);

        var auditInfo = new AuditInfo(
            createdAtUtc: changeContext.ChangedAtUtc,
            createdBy: changeContext.UserId);

        return new SupportCase(
            Guid.NewGuid(),
            NormalizeRequired(userRequest, nameof(UserRequest)),
            NormalizeRequired(problemDescription, nameof(ProblemDescription)),
            status,
            NormalizeOptional(moduleReference),
            NormalizeOptional(objectReference),
            CreateHistory("Created", status.ToString(), changeContext),
            triggeredBugId: null,
            triggeredIncidentId: null,
            auditInfo);
    }

    public void UpdateDetails(
        string userRequest,
        string problemDescription,
        ChangeContext changeContext,
        SupportCaseStatus status = SupportCaseStatus.Open,
        string? moduleReference = null,
        string? objectReference = null)
    {
        ValidateStatus(status);

        UserRequest = NormalizeRequired(userRequest, nameof(UserRequest));
        ProblemDescription = NormalizeRequired(problemDescription, nameof(ProblemDescription));
        Status = status;
        ModuleReference = NormalizeOptional(moduleReference) ?? ModuleReference;
        ObjectReference = NormalizeOptional(objectReference) ?? ObjectReference;
        HistoryLog = AppendHistory(HistoryLog, "Updated", status.ToString(), changeContext);

        Touch(changeContext);
    }

    public void LinkBug(Guid bugId, ChangeContext changeContext)
    {
        if (bugId == Guid.Empty)
        {
            throw new ValidationException("Bug id is required.");
        }

        TriggeredBugId = bugId;
        HistoryLog = AppendHistory(HistoryLog, "LinkedBug", bugId.ToString(), changeContext);
        Touch(changeContext);
    }

    public void LinkIncident(Guid incidentId, ChangeContext changeContext)
    {
        if (incidentId == Guid.Empty)
        {
            throw new ValidationException("Incident id is required.");
        }

        TriggeredIncidentId = incidentId;
        HistoryLog = AppendHistory(HistoryLog, "LinkedIncident", incidentId.ToString(), changeContext);
        Touch(changeContext);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static void ValidateStatus(SupportCaseStatus status)
    {
        if (!Enum.IsDefined(status))
        {
            throw new ValidationException("Support case status is invalid.");
        }
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

    private void Touch(ChangeContext changeContext)
    {
        AuditInfo = new AuditInfo(
            createdAtUtc: AuditInfo.CreatedAtUtc,
            createdBy: AuditInfo.CreatedBy,
            lastModifiedAtUtc: changeContext.ChangedAtUtc,
            lastModifiedBy: changeContext.UserId);
    }
}

public enum SupportCaseStatus
{
    Open = 0,
    InProgress = 1,
    WaitingForUser = 2,
    Resolved = 3,
    Closed = 4
}
