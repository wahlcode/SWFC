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
        AuditInfo = null!;
    }

    private ChangeRequest(
        Guid id,
        ChangeRequestType type,
        string description,
        string? requirementReference,
        string? roadmapReference,
        AuditInfo auditInfo)
    {
        Id = id;
        Type = type;
        Description = description;
        RequirementReference = requirementReference;
        RoadmapReference = roadmapReference;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public ChangeRequestType Type { get; private set; }
    public string Description { get; private set; }
    public string? RequirementReference { get; private set; }
    public string? RoadmapReference { get; private set; }
    public AuditInfo AuditInfo { get; private set; }

    public static ChangeRequest Create(
        ChangeRequestType type,
        string description,
        string? requirementReference,
        string? roadmapReference,
        ChangeContext changeContext)
    {
        ValidateType(type);

        var auditInfo = new AuditInfo(
            createdAtUtc: changeContext.ChangedAtUtc,
            createdBy: changeContext.UserId);

        return new ChangeRequest(
            Guid.NewGuid(),
            type,
            NormalizeRequired(description, nameof(Description)),
            NormalizeOptional(requirementReference),
            NormalizeOptional(roadmapReference),
            auditInfo);
    }

    public void UpdateDetails(
        ChangeRequestType type,
        string description,
        string? requirementReference,
        string? roadmapReference,
        ChangeContext changeContext)
    {
        ValidateType(type);

        Type = type;
        Description = NormalizeRequired(description, nameof(Description));
        RequirementReference = NormalizeOptional(requirementReference);
        RoadmapReference = NormalizeOptional(roadmapReference);

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
