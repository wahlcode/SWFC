using SWFC.Infrastructure.M400_Integration.M406_IdentityIntegration.Configuration;

namespace SWFC.Infrastructure.M400_Integration.M406_IdentityIntegration.Services;

public sealed record OidcProviderDescriptor(
    string DisplayName,
    string Authority,
    string ClientId,
    IReadOnlyList<string> Scopes,
    OidcClaimMappingOptions ClaimMapping);

public sealed class OidcProviderRegistry
{
    public OidcProviderDescriptor BuildDescriptor(OidcProviderOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (!options.IsConfigured())
        {
            throw new InvalidOperationException("OIDC provider is not configured.");
        }

        if (!Uri.TryCreate(options.Authority, UriKind.Absolute, out var authority) ||
            !string.Equals(authority.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("OIDC authority must be an absolute HTTPS URI.");
        }

        var scopes = options.Scopes
            .Where(scope => !string.IsNullOrWhiteSpace(scope))
            .Select(scope => scope.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (!scopes.Contains("openid", StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("OIDC scope 'openid' is required.");
        }

        return new OidcProviderDescriptor(
            options.DisplayName.Trim(),
            authority.ToString().TrimEnd('/'),
            options.ClientId.Trim(),
            scopes,
            options.ClaimMapping);
    }
}
