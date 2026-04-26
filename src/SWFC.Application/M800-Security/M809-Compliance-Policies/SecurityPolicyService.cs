using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M806_AccessControl.Decisions;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M800_Security.M809_CompliancePolicies;

public sealed record SecurityPolicy(
    string Code,
    string Name,
    string Scope,
    bool IsMandatory,
    IReadOnlyDictionary<string, string> Rules);

public sealed record PolicyEvaluationRequest(
    string PolicyCode,
    string ModuleCode,
    string ObjectType,
    string ObjectId,
    IReadOnlyDictionary<string, string> Facts,
    string Reason);

public sealed record PolicyEvaluationResult(
    bool IsCompliant,
    IReadOnlyList<string> Violations);

public interface ISecurityPolicyRepository
{
    Task<SecurityPolicy?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default);

    Task SaveAsync(
        SecurityPolicy policy,
        CancellationToken cancellationToken = default);
}

public interface ISecurityPolicyService
{
    Task<Result<PolicyEvaluationResult>> EvaluateAsync(
        SecurityContext actor,
        PolicyEvaluationRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> SavePolicyAsync(
        SecurityContext actor,
        SecurityPolicy policy,
        string reason,
        CancellationToken cancellationToken = default);
}

public sealed class SecurityPolicyService : ISecurityPolicyService
{
    private readonly ISecurityPolicyRepository _repository;
    private readonly IAccessDecisionService _accessDecisionService;
    private readonly IAuditService _auditService;

    public SecurityPolicyService(
        ISecurityPolicyRepository repository,
        IAccessDecisionService accessDecisionService,
        IAuditService auditService)
    {
        _repository = repository;
        _accessDecisionService = accessDecisionService;
        _auditService = auditService;
    }

    public async Task<Result<PolicyEvaluationResult>> EvaluateAsync(
        SecurityContext actor,
        PolicyEvaluationRequest request,
        CancellationToken cancellationToken = default)
    {
        var policy = await _repository.GetByCodeAsync(request.PolicyCode, cancellationToken);

        if (policy is null)
        {
            return Result<PolicyEvaluationResult>.Failure(new Error(
                "m809.policy.not_found",
                "Policy was not found.",
                ErrorCategory.NotFound));
        }

        var violations = policy.Rules
            .Where(rule => !request.Facts.TryGetValue(rule.Key, out var actual) ||
                           !string.Equals(actual, rule.Value, StringComparison.OrdinalIgnoreCase))
            .Select(rule => $"{rule.Key} must be {rule.Value}")
            .ToArray();

        var result = new PolicyEvaluationResult(violations.Length == 0, violations);

        await _auditService.WriteAsync(
            new AuditWriteRequest(
                ActorUserId: actor.UserId,
                ActorDisplayName: actor.DisplayName,
                Action: result.IsCompliant ? "PolicyCompliant" : "PolicyViolation",
                Module: "M809",
                ObjectType: request.ObjectType,
                ObjectId: request.ObjectId,
                TimestampUtc: DateTime.UtcNow,
                NewValues: $"Policy={policy.Code}; Scope={policy.Scope}; Violations={string.Join("|", violations)}",
                Reason: request.Reason),
            cancellationToken);

        return Result<PolicyEvaluationResult>.Success(result);
    }

    public async Task<Result> SavePolicyAsync(
        SecurityContext actor,
        SecurityPolicy policy,
        string reason,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(policy.Code) ||
            string.IsNullOrWhiteSpace(policy.Name) ||
            string.IsNullOrWhiteSpace(policy.Scope) ||
            policy.Rules.Count == 0 ||
            string.IsNullOrWhiteSpace(reason))
        {
            return Result.Failure(new Error(
                "m809.policy.invalid",
                "Policy code, name, scope, rules and reason are required.",
                ErrorCategory.Validation));
        }

        var decision = await _accessDecisionService.DecideAsync(
            actor,
            new AccessDecisionRequest(
                AccessAction.CanAdminister,
                "M809",
                "SecurityPolicy",
                policy.Code,
                requiredPermissions: new[] { "security.policies.write" }),
            cancellationToken);

        var audit = decision.ToAuditWriteRequest(actor, DateTime.UtcNow);
        await _auditService.WriteAsync(
            audit with { Reason = $"{audit.Reason}; Reason={reason}" },
            cancellationToken);

        if (!decision.IsAllowed)
        {
            return Result.Failure(new Error(
                "m809.policy.save.forbidden",
                decision.Reason,
                ErrorCategory.Security));
        }

        var existing = await _repository.GetByCodeAsync(policy.Code, cancellationToken);

        await _repository.SaveAsync(policy, cancellationToken);

        await _auditService.WriteAsync(
            new AuditWriteRequest(
                ActorUserId: actor.UserId,
                ActorDisplayName: actor.DisplayName,
                Action: existing is null ? "PolicyCreated" : "PolicyChanged",
                Module: "M809",
                ObjectType: "SecurityPolicy",
                ObjectId: policy.Code,
                TimestampUtc: DateTime.UtcNow,
                OldValues: existing is null ? null : $"Scope={existing.Scope}; Mandatory={existing.IsMandatory}",
                NewValues: $"Scope={policy.Scope}; Mandatory={policy.IsMandatory}; Rules={string.Join("|", policy.Rules.Keys)}",
                Reason: reason),
            cancellationToken);

        return Result.Success();
    }
}
