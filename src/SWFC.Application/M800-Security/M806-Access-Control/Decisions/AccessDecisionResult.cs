using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;

namespace SWFC.Application.M800_Security.M806_AccessControl.Decisions;

public sealed class AccessDecisionResult
{
    private AccessDecisionResult(
        bool isAllowed,
        bool approvalRequired,
        AccessAction action,
        string moduleCode,
        string objectType,
        string objectId,
        string reason,
        string? approvedByUserId)
    {
        IsAllowed = isAllowed;
        ApprovalRequired = approvalRequired;
        Action = action;
        ModuleCode = moduleCode;
        ObjectType = objectType;
        ObjectId = objectId;
        Reason = reason;
        ApprovedByUserId = approvedByUserId;
    }

    public bool IsAllowed { get; }
    public bool ApprovalRequired { get; }
    public AccessAction Action { get; }
    public string ModuleCode { get; }
    public string ObjectType { get; }
    public string ObjectId { get; }
    public string Reason { get; }
    public string? ApprovedByUserId { get; }

    public static AccessDecisionResult Allowed(AccessDecisionRequest request, string reason) =>
        new(
            true,
            false,
            request.Action,
            request.ModuleCode,
            request.ObjectType,
            request.ObjectId,
            reason,
            request.ApprovedByUserId);

    public static AccessDecisionResult Denied(AccessDecisionRequest request, string reason) =>
        new(
            false,
            false,
            request.Action,
            request.ModuleCode,
            request.ObjectType,
            request.ObjectId,
            reason,
            null);

    public static AccessDecisionResult NeedsApproval(AccessDecisionRequest request, string reason) =>
        new(
            false,
            true,
            request.Action,
            request.ModuleCode,
            request.ObjectType,
            request.ObjectId,
            reason,
            null);

    public AuditWriteRequest ToAuditWriteRequest(
        SecurityContext securityContext,
        DateTime timestampUtc)
    {
        return new AuditWriteRequest(
            ActorUserId: securityContext.UserId,
            ActorDisplayName: securityContext.DisplayName,
            Action: IsAllowed ? "AccessAllowed" : ApprovalRequired ? "AccessApprovalRequired" : "AccessDenied",
            Module: "M806",
            ObjectType: ObjectType,
            ObjectId: ObjectId,
            TimestampUtc: timestampUtc,
            TargetUserId: securityContext.UserId,
            NewValues: $"Action={Action}; Module={ModuleCode}; Result={(IsAllowed ? "Allowed" : "Denied")}",
            ApprovedByUserId: ApprovedByUserId,
            Reason: Reason);
    }
}
