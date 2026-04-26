using SWFC.Application.M1000_Platform.M1001_PluginSystem;

namespace SWFC.Application.M1100_ProductizationDistribution.M1101_Distribution;

public enum DistributionChannel
{
    Internal = 0,
    Customer = 1,
    Partner = 2
}

public sealed record DistributionProfile(
    string ProfileId,
    DistributionChannel Channel,
    string TargetContext,
    IReadOnlyCollection<string> RequiredReleaseApprovals,
    bool RequiresLicenseClearance);

public sealed record DistributionArtifact(
    string ArtifactId,
    string FileName,
    string Sha256,
    long SizeBytes);

public sealed record ProductDistributionRequest(
    string ProductCode,
    PluginVersion ProductVersion,
    DistributionProfile Profile,
    IReadOnlyCollection<DistributionArtifact> Artifacts,
    bool LicenseCleared);

public sealed record ProductDistributionPackage(
    string PackageId,
    string ProductCode,
    PluginVersion ProductVersion,
    string ProfileId,
    DistributionChannel Channel,
    IReadOnlyCollection<DistributionArtifact> Artifacts,
    IReadOnlyCollection<string> ReleaseApprovals,
    string Evidence);

public interface IProductDistributionService
{
    ProductDistributionPackage Prepare(ProductDistributionRequest request);
}

public sealed class ProductDistributionService : IProductDistributionService
{
    public ProductDistributionPackage Prepare(ProductDistributionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.ProductCode) ||
            string.IsNullOrWhiteSpace(request.Profile.ProfileId) ||
            string.IsNullOrWhiteSpace(request.Profile.TargetContext))
        {
            throw new ArgumentException("Product code, profile id and target context are required.");
        }

        if (request.Artifacts.Count == 0)
        {
            throw new ArgumentException("Distribution requires at least one artifact.");
        }

        if (request.Artifacts.Any(artifact =>
                string.IsNullOrWhiteSpace(artifact.ArtifactId) ||
                string.IsNullOrWhiteSpace(artifact.FileName) ||
                string.IsNullOrWhiteSpace(artifact.Sha256) ||
                artifact.SizeBytes <= 0))
        {
            throw new ArgumentException("Distribution artifacts require id, file name, digest and positive size.");
        }

        if (request.Profile.RequiredReleaseApprovals.Count == 0)
        {
            throw new ArgumentException("Distribution requires release approval references.");
        }

        if (request.Profile.RequiresLicenseClearance && !request.LicenseCleared)
        {
            throw new InvalidOperationException("Distribution profile requires license clearance.");
        }

        var packageId = $"{request.ProductCode.Trim()}-{request.ProductVersion}-{request.Profile.ProfileId.Trim()}";
        return new ProductDistributionPackage(
            packageId,
            request.ProductCode.Trim(),
            request.ProductVersion,
            request.Profile.ProfileId.Trim(),
            request.Profile.Channel,
            request.Artifacts
                .OrderBy(artifact => artifact.ArtifactId, StringComparer.Ordinal)
                .ToArray(),
            request.Profile.RequiredReleaseApprovals
                .Where(approval => !string.IsNullOrWhiteSpace(approval))
                .Select(approval => approval.Trim())
                .Order(StringComparer.Ordinal)
                .ToArray(),
            "Prepared product distribution package without setup, bootstrap or license decision ownership.");
    }
}
