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
        TriggeredBugId = null;
        TriggeredIncidentId = null;
        AuditInfo = null!;
    }

    private SupportCase(
        Guid id,
        string userRequest,
        string problemDescription,
        Guid? triggeredBugId,
        Guid? triggeredIncidentId,
        AuditInfo auditInfo)
    {
        Id = id;
        UserRequest = userRequest;
        ProblemDescription = problemDescription;
        TriggeredBugId = triggeredBugId;
        TriggeredIncidentId = triggeredIncidentId;
        AuditInfo = auditInfo;
    }

    public Guid Id { get; private set; }
    public string UserRequest { get; private set; }
    public string ProblemDescription { get; private set; }
    public Guid? TriggeredBugId { get; private set; }
    public Guid? TriggeredIncidentId { get; private set; }
    public AuditInfo AuditInfo { get; private set; }

    public static SupportCase Create(
        string userRequest,
        string problemDescription,
        ChangeContext changeContext)
    {
        var auditInfo = new AuditInfo(
            createdAtUtc: changeContext.ChangedAtUtc,
            createdBy: changeContext.UserId);

        return new SupportCase(
            Guid.NewGuid(),
            NormalizeRequired(userRequest, nameof(UserRequest)),
            NormalizeRequired(problemDescription, nameof(ProblemDescription)),
            triggeredBugId: null,
            triggeredIncidentId: null,
            auditInfo);
    }

    public void UpdateDetails(
        string userRequest,
        string problemDescription,
        ChangeContext changeContext)
    {
        UserRequest = NormalizeRequired(userRequest, nameof(UserRequest));
        ProblemDescription = NormalizeRequired(problemDescription, nameof(ProblemDescription));

        Touch(changeContext);
    }

    public void LinkBug(Guid bugId, ChangeContext changeContext)
    {
        if (bugId == Guid.Empty)
        {
            throw new ValidationException("Bug id is required.");
        }

        TriggeredBugId = bugId;
        Touch(changeContext);
    }

    public void LinkIncident(Guid incidentId, ChangeContext changeContext)
    {
        if (incidentId == Guid.Empty)
        {
            throw new ValidationException("Incident id is required.");
        }

        TriggeredIncidentId = incidentId;
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

    private void Touch(ChangeContext changeContext)
    {
        AuditInfo = new AuditInfo(
            createdAtUtc: AuditInfo.CreatedAtUtc,
            createdBy: AuditInfo.CreatedBy,
            lastModifiedAtUtc: changeContext.ChangedAtUtc,
            lastModifiedBy: changeContext.UserId);
    }
}
