namespace SWFC.Application.M1100_ProductizationDistribution.M1104_Licensing;

public enum ProductLicenseStatus
{
    Unknown = 0,
    Active = 1,
    Expired = 2,
    Suspended = 3
}

public sealed record ProductLicense(
    string LicenseId,
    string ProductCode,
    string CustomerContext,
    string LicenseType,
    DateTimeOffset ValidFromUtc,
    DateTimeOffset ValidUntilUtc,
    IReadOnlyCollection<string> EnabledProductFeatures,
    ProductLicenseStatus Status);

public sealed record ProductLicenseEvaluation(
    string LicenseId,
    ProductLicenseStatus Status,
    bool AllowsProductUse,
    IReadOnlyCollection<string> EnabledProductFeatures,
    string Evidence);

public interface IProductLicensingService
{
    ProductLicenseEvaluation Evaluate(ProductLicense license, DateTimeOffset atUtc);
}

public sealed class ProductLicensingService : IProductLicensingService
{
    public ProductLicenseEvaluation Evaluate(ProductLicense license, DateTimeOffset atUtc)
    {
        ArgumentNullException.ThrowIfNull(license);

        if (string.IsNullOrWhiteSpace(license.LicenseId) ||
            string.IsNullOrWhiteSpace(license.ProductCode) ||
            string.IsNullOrWhiteSpace(license.CustomerContext) ||
            string.IsNullOrWhiteSpace(license.LicenseType))
        {
            throw new ArgumentException("License id, product code, customer context and license type are required.");
        }

        var effectiveStatus = license.Status;
        if (effectiveStatus == ProductLicenseStatus.Active &&
            (atUtc < license.ValidFromUtc || atUtc > license.ValidUntilUtc))
        {
            effectiveStatus = ProductLicenseStatus.Expired;
        }

        return new ProductLicenseEvaluation(
            license.LicenseId.Trim(),
            effectiveStatus,
            effectiveStatus == ProductLicenseStatus.Active,
            license.EnabledProductFeatures
                .Where(feature => !string.IsNullOrWhiteSpace(feature))
                .Select(feature => feature.Trim())
                .Order(StringComparer.Ordinal)
                .ToArray(),
            "License evaluated only for product usage scope; authentication and authorization remain separate.");
    }
}
