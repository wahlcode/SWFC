using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Application.M800_Security.M806_AccessControl.Decisions;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M800_Security.M803_DataSecurity;

public enum SensitiveDataClassification
{
    Internal = 1,
    Confidential = 2,
    Secret = 3
}

public sealed record DataProtectionRequest(
    string ModuleCode,
    string ObjectType,
    string ObjectId,
    string DataName,
    string PlainText,
    SensitiveDataClassification Classification,
    string Reason);

public sealed record ProtectedDataPayload(
    string CipherText,
    string ProtectionScheme,
    string KeyVersion,
    SensitiveDataClassification Classification,
    DateTime ProtectedAtUtc);

public interface ISensitiveDataProtector
{
    ProtectedDataPayload Protect(
        string plainText,
        SensitiveDataClassification classification);

    string Reveal(ProtectedDataPayload payload);
}

public interface IDataProtectionService
{
    Task<Result<ProtectedDataPayload>> ProtectAsync(
        SecurityContext actor,
        DataProtectionRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<string>> RevealAsync(
        SecurityContext actor,
        string moduleCode,
        string objectType,
        string objectId,
        ProtectedDataPayload payload,
        string reason,
        CancellationToken cancellationToken = default);
}

public sealed class DataProtectionService : IDataProtectionService
{
    private readonly IAccessDecisionService _accessDecisionService;
    private readonly ISensitiveDataProtector _protector;
    private readonly IAuditService _auditService;

    public DataProtectionService(
        IAccessDecisionService accessDecisionService,
        ISensitiveDataProtector protector,
        IAuditService auditService)
    {
        _accessDecisionService = accessDecisionService;
        _protector = protector;
        _auditService = auditService;
    }

    public async Task<Result<ProtectedDataPayload>> ProtectAsync(
        SecurityContext actor,
        DataProtectionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.PlainText))
        {
            return Result<ProtectedDataPayload>.Failure(new Error(
                "m803.data.empty",
                "Sensitive data must not be empty.",
                ErrorCategory.Validation));
        }

        var decision = await _accessDecisionService.DecideAsync(
            actor,
            new AccessDecisionRequest(
                AccessAction.CanUpdate,
                request.ModuleCode,
                request.ObjectType,
                request.ObjectId,
                requiredPermissions: new[] { "data.protect" }),
            cancellationToken);

        await AuditDecisionAsync(actor, decision, request.Reason, cancellationToken);

        if (!decision.IsAllowed)
        {
            return Result<ProtectedDataPayload>.Failure(new Error(
                "m803.data.protect.forbidden",
                decision.Reason,
                ErrorCategory.Security));
        }

        var protectedPayload = _protector.Protect(request.PlainText, request.Classification);

        if (string.Equals(protectedPayload.CipherText, request.PlainText, StringComparison.Ordinal))
        {
            return Result<ProtectedDataPayload>.Failure(new Error(
                "m803.data.protection.invalid",
                "Sensitive data protector returned unprotected clear text.",
                ErrorCategory.Security));
        }

        await _auditService.WriteAsync(
            new AuditWriteRequest(
                ActorUserId: actor.UserId,
                ActorDisplayName: actor.DisplayName,
                Action: "DataProtected",
                Module: "M803",
                ObjectType: request.ObjectType,
                ObjectId: request.ObjectId,
                TimestampUtc: DateTime.UtcNow,
                NewValues: $"Data={request.DataName}; Classification={request.Classification}; Scheme={protectedPayload.ProtectionScheme}; KeyVersion={protectedPayload.KeyVersion}",
                Reason: request.Reason),
            cancellationToken);

        return Result<ProtectedDataPayload>.Success(protectedPayload);
    }

    public async Task<Result<string>> RevealAsync(
        SecurityContext actor,
        string moduleCode,
        string objectType,
        string objectId,
        ProtectedDataPayload payload,
        string reason,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            return Result<string>.Failure(new Error(
                "m803.data.reveal.reason_required",
                "A reason is required to reveal sensitive data.",
                ErrorCategory.Validation));
        }

        var decision = await _accessDecisionService.DecideAsync(
            actor,
            new AccessDecisionRequest(
                AccessAction.CanView,
                moduleCode,
                objectType,
                objectId,
                requiredPermissions: new[] { "data.reveal" }),
            cancellationToken);

        await AuditDecisionAsync(actor, decision, reason, cancellationToken);

        if (!decision.IsAllowed)
        {
            return Result<string>.Failure(new Error(
                "m803.data.reveal.forbidden",
                decision.Reason,
                ErrorCategory.Security));
        }

        var revealed = _protector.Reveal(payload);

        await _auditService.WriteAsync(
            new AuditWriteRequest(
                ActorUserId: actor.UserId,
                ActorDisplayName: actor.DisplayName,
                Action: "DataRevealed",
                Module: "M803",
                ObjectType: objectType,
                ObjectId: objectId,
                TimestampUtc: DateTime.UtcNow,
                NewValues: $"Classification={payload.Classification}; Scheme={payload.ProtectionScheme}; KeyVersion={payload.KeyVersion}",
                Reason: reason),
            cancellationToken);

        return Result<string>.Success(revealed);
    }

    private Task AuditDecisionAsync(
        SecurityContext actor,
        AccessDecisionResult decision,
        string reason,
        CancellationToken cancellationToken)
    {
        var request = decision.ToAuditWriteRequest(actor, DateTime.UtcNow);

        return _auditService.WriteAsync(
            request with
            {
                Reason = string.IsNullOrWhiteSpace(reason)
                    ? request.Reason
                    : $"{request.Reason}; Reason={reason}"
            },
            cancellationToken);
    }
}
