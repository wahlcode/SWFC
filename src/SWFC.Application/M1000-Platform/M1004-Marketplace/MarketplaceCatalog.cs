using SWFC.Application.M1000_Platform.M1001_PluginSystem;

namespace SWFC.Application.M1000_Platform.M1004_Marketplace;

public sealed record MarketplaceExtensionPackage(
    string PackageId,
    string ExtensionId,
    string DisplayName,
    PluginVersion Version,
    IReadOnlyCollection<string> Categories,
    PluginVersion MinimumPlatformVersion,
    PluginVersion MaximumPlatformVersion,
    bool IsApprovedForDistribution);

public sealed record MarketplaceSearchRequest(
    string? Category,
    PluginVersion PlatformVersion,
    bool ApprovedOnly);

public sealed record MarketplacePackageReference(
    string PackageId,
    string ExtensionId,
    string DisplayName,
    PluginVersion Version,
    bool IsCompatible,
    string DistributionReference);

public interface IMarketplaceCatalog
{
    void Publish(MarketplaceExtensionPackage package);

    IReadOnlyList<MarketplacePackageReference> Search(MarketplaceSearchRequest request);
}

public sealed class MarketplaceCatalog : IMarketplaceCatalog
{
    private readonly Dictionary<string, MarketplaceExtensionPackage> _packages = new(StringComparer.Ordinal);

    public void Publish(MarketplaceExtensionPackage package)
    {
        ArgumentNullException.ThrowIfNull(package);

        if (string.IsNullOrWhiteSpace(package.PackageId) ||
            string.IsNullOrWhiteSpace(package.ExtensionId) ||
            string.IsNullOrWhiteSpace(package.DisplayName))
        {
            throw new ArgumentException("Package id, extension id and display name are required.");
        }

        _packages[package.PackageId.Trim()] = package with
        {
            PackageId = package.PackageId.Trim(),
            ExtensionId = package.ExtensionId.Trim(),
            DisplayName = package.DisplayName.Trim(),
            Categories = package.Categories
                .Where(category => !string.IsNullOrWhiteSpace(category))
                .Select(category => category.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Order(StringComparer.OrdinalIgnoreCase)
                .ToArray()
        };
    }

    public IReadOnlyList<MarketplacePackageReference> Search(MarketplaceSearchRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return _packages.Values
            .Where(package => !request.ApprovedOnly || package.IsApprovedForDistribution)
            .Where(package => request.Category is null ||
                              package.Categories.Contains(request.Category, StringComparer.OrdinalIgnoreCase))
            .OrderBy(package => package.DisplayName, StringComparer.Ordinal)
            .ThenByDescending(package => package.Version)
            .Select(package => new MarketplacePackageReference(
                package.PackageId,
                package.ExtensionId,
                package.DisplayName,
                package.Version,
                request.PlatformVersion.CompareTo(package.MinimumPlatformVersion) >= 0 &&
                request.PlatformVersion.CompareTo(package.MaximumPlatformVersion) <= 0,
                $"marketplace://extensions/{package.PackageId}"))
            .ToArray();
    }
}
