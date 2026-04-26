using SWFC.Application.M1000_Platform.M1001_PluginSystem;

namespace SWFC.Application.M1000_Platform.M1002_DeveloperPlatformSdk;

public sealed record SdkApiContract(
    string ApiCode,
    string Description,
    IReadOnlyCollection<string> AllowedOperations);

public sealed record ExtensionDevelopmentProfile(
    string DeveloperId,
    string ExtensionId,
    PluginVersion TargetPlatformVersion,
    IReadOnlyCollection<string> RequestedApiCodes,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record SdkValidationReport(
    string ExtensionId,
    bool IsCompliant,
    IReadOnlyCollection<string> Violations,
    IReadOnlyCollection<string> ApprovedApiCodes,
    string Evidence);

public sealed record PluginProjectDescriptor(
    string ExtensionId,
    string ManifestFileName,
    IReadOnlyCollection<string> RequiredFiles,
    IReadOnlyCollection<string> ApiContracts);

public interface IDeveloperPlatformSdk
{
    void RegisterContract(SdkApiContract contract);

    SdkValidationReport Validate(ExtensionDevelopmentProfile profile);

    PluginProjectDescriptor CreateProjectDescriptor(ExtensionDevelopmentProfile profile);
}

public sealed class DeveloperPlatformSdk : IDeveloperPlatformSdk
{
    private readonly Dictionary<string, SdkApiContract> _contracts = new(StringComparer.Ordinal);

    public DeveloperPlatformSdk()
    {
        RegisterContract(new SdkApiContract(
            "M1001.PluginContracts",
            "Plugin manifest, capability and lifecycle contracts.",
            ["DeclareManifest", "Initialize", "Shutdown"]));
        RegisterContract(new SdkApiContract(
            "M1003.ExtensionLifecycle",
            "Extension lifecycle state transitions through management contracts.",
            ["Install", "Activate", "Deactivate", "Inspect"]));
    }

    public void RegisterContract(SdkApiContract contract)
    {
        ArgumentNullException.ThrowIfNull(contract);

        if (string.IsNullOrWhiteSpace(contract.ApiCode) ||
            string.IsNullOrWhiteSpace(contract.Description) ||
            contract.AllowedOperations.Count == 0)
        {
            throw new ArgumentException("API code, description and operations are required.");
        }

        _contracts[contract.ApiCode.Trim()] = contract with
        {
            ApiCode = contract.ApiCode.Trim(),
            Description = contract.Description.Trim(),
            AllowedOperations = contract.AllowedOperations
                .Where(operation => !string.IsNullOrWhiteSpace(operation))
                .Select(operation => operation.Trim())
                .Order(StringComparer.Ordinal)
                .ToArray()
        };
    }

    public SdkValidationReport Validate(ExtensionDevelopmentProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var violations = new List<string>();
        if (string.IsNullOrWhiteSpace(profile.DeveloperId))
        {
            violations.Add("Developer id is required.");
        }

        if (string.IsNullOrWhiteSpace(profile.ExtensionId))
        {
            violations.Add("Extension id is required.");
        }

        var requestedApis = profile.RequestedApiCodes
            .Select(api => api.Trim())
            .Where(api => api.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        foreach (var apiCode in requestedApis.Where(apiCode => !_contracts.ContainsKey(apiCode)))
        {
            violations.Add($"Requested API '{apiCode}' is not part of the SDK contract set.");
        }

        if (!profile.Metadata.TryGetValue("UsesOnlyPlatformContracts", out var usesContracts) ||
            !string.Equals(usesContracts, "true", StringComparison.OrdinalIgnoreCase))
        {
            violations.Add("Extension must declare use of platform contracts only.");
        }

        var approvedApis = requestedApis.Where(apiCode => _contracts.ContainsKey(apiCode)).ToArray();
        return new SdkValidationReport(
            profile.ExtensionId,
            violations.Count == 0,
            violations,
            approvedApis,
            $"Validated {requestedApis.Length} requested APIs against {_contracts.Count} SDK contracts.");
    }

    public PluginProjectDescriptor CreateProjectDescriptor(ExtensionDevelopmentProfile profile)
    {
        var validation = Validate(profile);
        if (!validation.IsCompliant)
        {
            throw new InvalidOperationException($"Extension '{profile.ExtensionId}' is not SDK compliant.");
        }

        return new PluginProjectDescriptor(
            profile.ExtensionId.Trim(),
            "plugin.manifest.json",
            [
                "plugin.manifest.json",
                "src/ExtensionEntry.cs",
                "docs/extension-contracts.md"
            ],
            validation.ApprovedApiCodes);
    }
}
