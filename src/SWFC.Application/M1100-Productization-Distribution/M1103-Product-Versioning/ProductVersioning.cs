using SWFC.Application.M1000_Platform.M1001_PluginSystem;

namespace SWFC.Application.M1100_ProductizationDistribution.M1103_ProductVersioning;

public enum ProductVersionReleaseState
{
    Planned = 0,
    Released = 1,
    Installed = 2,
    Retired = 3
}

public sealed record ProductVersionRecord(
    string ProductCode,
    PluginVersion ProductVersion,
    PluginVersion PlatformVersion,
    ProductVersionReleaseState State,
    string CustomerContext,
    DateTimeOffset ChangedAtUtc);

public interface IProductVersionRegistry
{
    ProductVersionRecord Record(ProductVersionRecord versionRecord);

    IReadOnlyList<ProductVersionRecord> GetHistory(string productCode);

    ProductVersionRecord? GetInstalled(string productCode, string customerContext);
}

public sealed class ProductVersionRegistry : IProductVersionRegistry
{
    private readonly List<ProductVersionRecord> _history = [];

    public ProductVersionRecord Record(ProductVersionRecord versionRecord)
    {
        ArgumentNullException.ThrowIfNull(versionRecord);

        if (string.IsNullOrWhiteSpace(versionRecord.ProductCode) ||
            string.IsNullOrWhiteSpace(versionRecord.CustomerContext))
        {
            throw new ArgumentException("Product code and customer context are required.");
        }

        var normalized = versionRecord with
        {
            ProductCode = versionRecord.ProductCode.Trim(),
            CustomerContext = versionRecord.CustomerContext.Trim()
        };

        _history.Add(normalized);
        return normalized;
    }

    public IReadOnlyList<ProductVersionRecord> GetHistory(string productCode) =>
        _history
            .Where(record => string.Equals(record.ProductCode, productCode, StringComparison.Ordinal))
            .OrderBy(record => record.ChangedAtUtc)
            .ToArray();

    public ProductVersionRecord? GetInstalled(string productCode, string customerContext) =>
        _history
            .Where(record =>
                record.State == ProductVersionReleaseState.Installed &&
                string.Equals(record.ProductCode, productCode, StringComparison.Ordinal) &&
                string.Equals(record.CustomerContext, customerContext, StringComparison.Ordinal))
            .OrderByDescending(record => record.ChangedAtUtc)
            .FirstOrDefault();
}
