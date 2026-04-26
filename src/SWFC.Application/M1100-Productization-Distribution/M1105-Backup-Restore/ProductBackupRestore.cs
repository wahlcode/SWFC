using System.Security.Cryptography;
using System.Text;
using SWFC.Application.M1000_Platform.M1001_PluginSystem;

namespace SWFC.Application.M1100_ProductizationDistribution.M1105_BackupRestore;

public enum BackupRestoreStatus
{
    Prepared = 0,
    Verified = 1,
    Rejected = 2
}

public sealed record BackupScope(
    string ProductCode,
    string CustomerContext,
    PluginVersion ProductVersion,
    IReadOnlyCollection<string> Components);

public sealed record BackupSnapshot(
    string BackupId,
    BackupScope Scope,
    DateTimeOffset CreatedAtUtc,
    BackupRestoreStatus Status,
    string IntegrityHash,
    string Evidence);

public sealed record RestoreRequest(
    string RestoreId,
    BackupSnapshot Snapshot,
    string TargetContext,
    bool OperatorApproved);

public sealed record RestorePlan(
    string RestoreId,
    BackupRestoreStatus Status,
    IReadOnlyCollection<string> Steps,
    string Evidence);

public interface IProductBackupRestoreService
{
    BackupSnapshot CreateSnapshot(BackupScope scope, DateTimeOffset createdAtUtc);

    RestorePlan PrepareRestore(RestoreRequest request);
}

public sealed class ProductBackupRestoreService : IProductBackupRestoreService
{
    public BackupSnapshot CreateSnapshot(BackupScope scope, DateTimeOffset createdAtUtc)
    {
        ArgumentNullException.ThrowIfNull(scope);

        if (string.IsNullOrWhiteSpace(scope.ProductCode) ||
            string.IsNullOrWhiteSpace(scope.CustomerContext) ||
            scope.Components.Count == 0)
        {
            throw new ArgumentException("Backup scope requires product code, customer context and components.");
        }

        var normalizedComponents = scope.Components
            .Where(component => !string.IsNullOrWhiteSpace(component))
            .Select(component => component.Trim())
            .Order(StringComparer.Ordinal)
            .ToArray();
        var normalizedScope = scope with
        {
            ProductCode = scope.ProductCode.Trim(),
            CustomerContext = scope.CustomerContext.Trim(),
            Components = normalizedComponents
        };
        var material = $"{normalizedScope.ProductCode}|{normalizedScope.CustomerContext}|{normalizedScope.ProductVersion}|{string.Join(",", normalizedComponents)}|{createdAtUtc:O}";
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(material)));

        return new BackupSnapshot(
            $"backup-{hash[..12].ToLowerInvariant()}",
            normalizedScope,
            createdAtUtc,
            BackupRestoreStatus.Verified,
            hash,
            "Backup snapshot prepared with deterministic integrity evidence; no bootstrap or versioning ownership is assumed.");
    }

    public RestorePlan PrepareRestore(RestoreRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var violations = new List<string>();
        if (string.IsNullOrWhiteSpace(request.RestoreId) || string.IsNullOrWhiteSpace(request.TargetContext))
        {
            violations.Add("Restore id and target context are required.");
        }

        if (request.Snapshot.Status != BackupRestoreStatus.Verified)
        {
            violations.Add("Snapshot must be verified before restore.");
        }

        if (!request.OperatorApproved)
        {
            violations.Add("Restore requires operator approval.");
        }

        var status = violations.Count == 0 ? BackupRestoreStatus.Prepared : BackupRestoreStatus.Rejected;
        var steps = status == BackupRestoreStatus.Prepared
            ? new[]
            {
                "Verify backup integrity hash.",
                "Lock product operations context.",
                "Restore product components in recorded order.",
                "Record restore evidence for operations."
            }
            : violations.ToArray();

        return new RestorePlan(
            request.RestoreId,
            status,
            steps,
            status == BackupRestoreStatus.Prepared
                ? "Restore plan prepared in product backup/restore context."
                : "Restore plan rejected by product backup/restore controls.");
    }
}
