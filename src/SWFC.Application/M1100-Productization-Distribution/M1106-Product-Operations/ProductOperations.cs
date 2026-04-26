using SWFC.Application.M1100_ProductizationDistribution.M1102_Updates;
using SWFC.Application.M1100_ProductizationDistribution.M1105_BackupRestore;

namespace SWFC.Application.M1100_ProductizationDistribution.M1106_ProductOperations;

public enum ProductOperationsStatus
{
    Unknown = 0,
    Operational = 1,
    AttentionRequired = 2,
    Maintenance = 3
}

public sealed record ProductOperationsSignal(
    string SourceModule,
    string SignalCode,
    string Summary,
    ProductOperationsStatus Status);

public sealed record ProductOperationsView(
    string ProductCode,
    string CustomerContext,
    ProductOperationsStatus Status,
    IReadOnlyCollection<ProductOperationsSignal> Signals,
    string Evidence);

public interface IProductOperationsService
{
    ProductOperationsView BuildView(
        string productCode,
        string customerContext,
        IReadOnlyCollection<ProductOperationsSignal> signals);

    ProductOperationsSignal FromUpdateDecision(ProductUpdateRolloutDecision decision);

    ProductOperationsSignal FromBackupSnapshot(BackupSnapshot snapshot);
}

public sealed class ProductOperationsService : IProductOperationsService
{
    public ProductOperationsView BuildView(
        string productCode,
        string customerContext,
        IReadOnlyCollection<ProductOperationsSignal> signals)
    {
        if (string.IsNullOrWhiteSpace(productCode) || string.IsNullOrWhiteSpace(customerContext))
        {
            throw new ArgumentException("Product code and customer context are required.");
        }

        var normalizedSignals = signals
            .OrderBy(signal => signal.SourceModule, StringComparer.Ordinal)
            .ThenBy(signal => signal.SignalCode, StringComparer.Ordinal)
            .ToArray();

        var status = normalizedSignals.Any(signal => signal.Status == ProductOperationsStatus.AttentionRequired)
            ? ProductOperationsStatus.AttentionRequired
            : normalizedSignals.Any(signal => signal.Status == ProductOperationsStatus.Maintenance)
                ? ProductOperationsStatus.Maintenance
                : ProductOperationsStatus.Operational;

        return new ProductOperationsView(
            productCode.Trim(),
            customerContext.Trim(),
            status,
            normalizedSignals,
            "Operations view references product signals without replacing runtime, security or control-desk responsibilities.");
    }

    public ProductOperationsSignal FromUpdateDecision(ProductUpdateRolloutDecision decision)
    {
        ArgumentNullException.ThrowIfNull(decision);

        var status = decision.Status == ProductUpdateRolloutStatus.Blocked
            ? ProductOperationsStatus.AttentionRequired
            : decision.Status == ProductUpdateRolloutStatus.Distributed
                ? ProductOperationsStatus.Operational
                : ProductOperationsStatus.Maintenance;

        return new ProductOperationsSignal(
            "M1102",
            decision.UpdateId,
            $"Product update rollout status: {decision.Status}.",
            status);
    }

    public ProductOperationsSignal FromBackupSnapshot(BackupSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        return new ProductOperationsSignal(
            "M1105",
            snapshot.BackupId,
            $"Backup status: {snapshot.Status}.",
            snapshot.Status == BackupRestoreStatus.Verified
                ? ProductOperationsStatus.Operational
                : ProductOperationsStatus.AttentionRequired);
    }
}
