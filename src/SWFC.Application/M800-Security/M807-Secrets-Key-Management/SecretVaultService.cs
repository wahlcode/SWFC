using SWFC.Application.M800_Security.M803_DataSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Application.M800_Security.M806_AccessControl.Decisions;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M800_Security.M807_SecretsKeyManagement;

public sealed class SecretVaultService : ISecretVaultService
{
    private readonly ISecretVaultRepository _repository;
    private readonly ISensitiveDataProtector _protector;
    private readonly IAccessDecisionService _accessDecisionService;
    private readonly IAuditService _auditService;

    public SecretVaultService(
        ISecretVaultRepository repository,
        ISensitiveDataProtector protector,
        IAccessDecisionService accessDecisionService,
        IAuditService auditService)
    {
        _repository = repository;
        _protector = protector;
        _accessDecisionService = accessDecisionService;
        _auditService = auditService;
    }

    public async Task<Result<SecretDescriptor>> StoreAsync(
        SecurityContext actor,
        string name,
        SecretKind kind,
        string plainTextValue,
        string reason,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(plainTextValue) || string.IsNullOrWhiteSpace(reason))
        {
            return Result<SecretDescriptor>.Failure(new Error(
                "m807.secret.invalid",
                "Secret name, value and reason are required.",
                ErrorCategory.Validation));
        }

        var decision = await AuthorizeAsync(actor, AccessAction.CanCreate, name, "security.secrets.write", reason, cancellationToken);

        if (!decision.IsAllowed)
        {
            return Result<SecretDescriptor>.Failure(new Error(
                "m807.secret.store.forbidden",
                decision.Reason,
                ErrorCategory.Security));
        }

        var existing = await _repository.GetActiveByNameAsync(name.Trim(), cancellationToken);
        var version = existing is null ? 1 : existing.Version + 1;
        var payload = _protector.Protect(plainTextValue, SensitiveDataClassification.Secret);

        if (string.Equals(payload.CipherText, plainTextValue, StringComparison.Ordinal))
        {
            return Result<SecretDescriptor>.Failure(new Error(
                "m807.secret.unprotected",
                "Secrets must never be stored as clear text.",
                ErrorCategory.Security));
        }

        if (existing is not null)
        {
            await _repository.DeactivateAsync(existing.Id, cancellationToken);
        }

        var secret = new StoredSecret(
            Guid.NewGuid(),
            name.Trim(),
            kind,
            version,
            payload,
            true,
            DateTime.UtcNow);

        await _repository.AddAsync(secret, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        await _auditService.WriteAsync(
            new AuditWriteRequest(
                ActorUserId: actor.UserId,
                ActorDisplayName: actor.DisplayName,
                Action: existing is null ? "SecretStored" : "SecretRotated",
                Module: "M807",
                ObjectType: "Secret",
                ObjectId: secret.Name,
                TimestampUtc: DateTime.UtcNow,
                NewValues: $"Kind={secret.Kind}; Version={secret.Version}; Scheme={payload.ProtectionScheme}; KeyVersion={payload.KeyVersion}",
                Reason: reason),
            cancellationToken);

        return Result<SecretDescriptor>.Success(ToDescriptor(secret));
    }

    public async Task<Result<string>> RetrieveAsync(
        SecurityContext actor,
        string name,
        string reason,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            return Result<string>.Failure(new Error(
                "m807.secret.retrieve.reason_required",
                "A reason is required to access secrets.",
                ErrorCategory.Validation));
        }

        var decision = await AuthorizeAsync(actor, AccessAction.CanView, name, "security.secrets.read", reason, cancellationToken);

        if (!decision.IsAllowed)
        {
            return Result<string>.Failure(new Error(
                "m807.secret.retrieve.forbidden",
                decision.Reason,
                ErrorCategory.Security));
        }

        var secret = await _repository.GetActiveByNameAsync(name.Trim(), cancellationToken);

        if (secret is null)
        {
            return Result<string>.Failure(new Error(
                "m807.secret.not_found",
                "Secret was not found.",
                ErrorCategory.NotFound));
        }

        var revealed = _protector.Reveal(secret.ProtectedValue);

        await _auditService.WriteAsync(
            new AuditWriteRequest(
                ActorUserId: actor.UserId,
                ActorDisplayName: actor.DisplayName,
                Action: "SecretAccessed",
                Module: "M807",
                ObjectType: "Secret",
                ObjectId: secret.Name,
                TimestampUtc: DateTime.UtcNow,
                NewValues: $"Kind={secret.Kind}; Version={secret.Version}",
                Reason: reason),
            cancellationToken);

        return Result<string>.Success(revealed);
    }

    public Task<Result<SecretDescriptor>> RotateAsync(
        SecurityContext actor,
        string name,
        string newPlainTextValue,
        string reason,
        CancellationToken cancellationToken = default)
    {
        return StoreAsync(actor, name, SecretKind.Token, newPlainTextValue, reason, cancellationToken);
    }

    private async Task<AccessDecisionResult> AuthorizeAsync(
        SecurityContext actor,
        AccessAction action,
        string secretName,
        string permission,
        string reason,
        CancellationToken cancellationToken)
    {
        var decision = await _accessDecisionService.DecideAsync(
            actor,
            new AccessDecisionRequest(
                action,
                "M807",
                "Secret",
                string.IsNullOrWhiteSpace(secretName) ? "unknown" : secretName.Trim(),
                requiredPermissions: new[] { permission }),
            cancellationToken);

        var audit = decision.ToAuditWriteRequest(actor, DateTime.UtcNow);
        await _auditService.WriteAsync(
            audit with { Reason = $"{audit.Reason}; Reason={reason}" },
            cancellationToken);

        return decision;
    }

    private static SecretDescriptor ToDescriptor(StoredSecret secret) =>
        new(secret.Id, secret.Name, secret.Kind, secret.Version, secret.IsActive, secret.CreatedAtUtc);
}
