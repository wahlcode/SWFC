using SWFC.Domain.M100_System.M101_Foundation.Exceptions;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;

namespace SWFC.Domain.M700_Support.M702_ChangeRequests;

public sealed class ChangeRequest
{
    private ChangeRequest()
    {
        Id = Guid.Empty;
        Type = ChangeRequestType.ChangeRequest;
        Description = string.Empty;
        RequirementReference = null;
        RoadmapReference = null;
        Status = ChangeRequestStatus.Open;
        ModuleReference = null;
        ObjectReference = null;
        HistoryLog = string.Empty;
        AuditInfo = null!;
    }

    private ChangeRequest(
        Guid id,
        ChangeRequestType type,
        string description,
        string? requirementReference,
        string? roadmapReference,
        ChangeRequestStatus status,
        string? moduleReference,
        string? objectReference,
        string historyLog,
        AuditInfo auditInfo)
    {
        Id = id;
        Type = type;
        Description = description;
        RequirementReference = requirementReference;
        RoadmapReference = roadmapReference;
        Status = status;
        ModuleReference = moduleReference;
        ObjectReference = objectReference;
        HistoryLog = historyLog;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public ChangeRequestType Type { get; private set; }
    public string Description { get; private set; }
    public string? RequirementReference { get; private set; }
    public string? RoadmapReference { get; private set; }
    public ChangeRequestStatus Status { get; private set; }
    public string? ModuleReference { get; private set; }
    public string? ObjectReference { get; private set; }
    public string HistoryLog { get; private set; }
    public AuditInfo AuditInfo { get; private set; }

    public static ChangeRequest Create(
        ChangeRequestType type,
        string description,
        string? requirementReference,
        string? roadmapReference,
        ChangeContext changeContext,
        ChangeRequestStatus status = ChangeRequestStatus.Open,
        string? moduleReference = null,
        string? objectReference = null)
    {
        ValidateType(type);
        ValidateStatus(status);

        var auditInfo = new AuditInfo(
            createdAtUtc: changeContext.ChangedAtUtc,
            createdBy: changeContext.UserId);

        return new ChangeRequest(
            Guid.NewGuid(),
            type,
            NormalizeRequired(description, nameof(Description)),
            NormalizeOptional(requirementReference),
            NormalizeOptional(roadmapReference),
            status,
            NormalizeOptional(moduleReference),
            NormalizeOptional(objectReference),
            CreateHistory("Created", status.ToString(), changeContext),
            auditInfo);
    }

    public void UpdateDetails(
        ChangeRequestType type,
        string description,
        string? requirementReference,
        string? roadmapReference,
        ChangeContext changeContext,
        ChangeRequestStatus status = ChangeRequestStatus.Open,
        string? moduleReference = null,
        string? objectReference = null)
    {
        ValidateType(type);
        ValidateStatus(status);

        Type = type;
        Description = NormalizeRequired(description, nameof(Description));
        RequirementReference = NormalizeOptional(requirementReference);
        RoadmapReference = NormalizeOptional(roadmapReference);
        Status = status;
        ModuleReference = NormalizeOptional(moduleReference) ?? ModuleReference;
        ObjectReference = NormalizeOptional(objectReference) ?? ObjectReference;
        HistoryLog = AppendHistory(HistoryLog, "Updated", status.ToString(), changeContext);

        Touch(changeContext);
    }

    private static void ValidateStatus(ChangeRequestStatus status)
    {
        if (!Enum.IsDefined(status))
        {
            throw new ValidationException("Change request status is invalid.");
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

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static void ValidateType(ChangeRequestType type)
    {
        if (!Enum.IsDefined(type))
        {
            throw new ValidationException("Change request type is invalid.");
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

public enum ChangeRequestType
{
    ChangeRequest = 0,
    ImprovementIdea = 1,
    Extension = 2
}

public enum ChangeRequestStatus
{
    Open = 0,
    InReview = 1,
    Accepted = 2,
    Rejected = 3,
    LinkedToRequirement = 4
}
