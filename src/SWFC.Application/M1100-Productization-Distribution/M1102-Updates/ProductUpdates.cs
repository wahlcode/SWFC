using SWFC.Application.M1000_Platform.M1001_PluginSystem;
using SWFC.Application.M1000_Platform.M1005_VersioningUpdateManagement;

namespace SWFC.Application.M1100_ProductizationDistribution.M1102_Updates;

public enum ProductUpdateRolloutStatus
{
    Draft = 0,
    Released = 1,
    Blocked = 2,
    Distributed = 3
}

public sealed record ProductUpdatePackage(
    string UpdateId,
    PluginVersion ProductFromVersion,
    PluginVersion ProductToVersion,
    PluginVersion RequiredPlatformVersion,
    IReadOnlyCollection<string> TargetGroups,
    bool ProductReleaseApproved);

public sealed record ProductUpdateRolloutDecision(
    string UpdateId,
    ProductUpdateRolloutStatus Status,
    IReadOnlyCollection<string> Reasons,
    IReadOnlyCollection<string> TargetGroups);

public interface IProductUpdateOrchestrator
{
    ProductUpdateRolloutDecision Release(
        ProductUpdatePackage package,
        PlatformVersionState platformState);

    ProductUpdateRolloutDecision MarkDistributed(string updateId);

    IReadOnlyList<ProductUpdateRolloutDecision> GetHistory();
}

public sealed class ProductUpdateOrchestrator : IProductUpdateOrchestrator
{
    private readonly Dictionary<string, ProductUpdatePackage> _releasedUpdates = new(StringComparer.Ordinal);
    private readonly List<ProductUpdateRolloutDecision> _history = [];

    public ProductUpdateRolloutDecision Release(
        ProductUpdatePackage package,
        PlatformVersionState platformState)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(platformState);

        var reasons = new List<string>();
        if (string.IsNullOrWhiteSpace(package.UpdateId))
        {
            reasons.Add("Update id is required.");
        }

        if (package.ProductToVersion.CompareTo(package.ProductFromVersion) <= 0)
        {
            reasons.Add("Product target version must be greater than product source version.");
        }

        if (platformState.Version.CompareTo(package.RequiredPlatformVersion) < 0)
        {
            reasons.Add($"Platform version {platformState.Version} is below required version {package.RequiredPlatformVersion}.");
        }

        if (package.TargetGroups.Count == 0)
        {
            reasons.Add("At least one product target group is required.");
        }

        if (!package.ProductReleaseApproved)
        {
            reasons.Add("Product release approval is required.");
        }

        var status = reasons.Count == 0
            ? ProductUpdateRolloutStatus.Released
            : ProductUpdateRolloutStatus.Blocked;

        var decision = new ProductUpdateRolloutDecision(
            package.UpdateId,
            status,
            reasons,
            package.TargetGroups
                .Where(group => !string.IsNullOrWhiteSpace(group))
                .Select(group => group.Trim())
                .Order(StringComparer.Ordinal)
                .ToArray());

        if (status == ProductUpdateRolloutStatus.Released)
        {
            _releasedUpdates[package.UpdateId] = package;
        }

        _history.Add(decision);
        return decision;
    }

    public ProductUpdateRolloutDecision MarkDistributed(string updateId)
    {
        if (!_releasedUpdates.TryGetValue(updateId, out var package))
        {
            throw new InvalidOperationException($"Update '{updateId}' is not released for distribution.");
        }

        var decision = new ProductUpdateRolloutDecision(
            updateId,
            ProductUpdateRolloutStatus.Distributed,
            ["Product update distributed to approved target groups."],
            package.TargetGroups
                .Order(StringComparer.Ordinal)
                .ToArray());
        _history.Add(decision);
        return decision;
    }

    public IReadOnlyList<ProductUpdateRolloutDecision> GetHistory() => _history.ToArray();
}
