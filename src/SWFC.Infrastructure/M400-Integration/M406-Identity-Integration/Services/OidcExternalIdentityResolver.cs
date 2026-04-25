using System.Security.Claims;
using Microsoft.Extensions.Options;
using SWFC.Infrastructure.M400_Integration.M406_IdentityIntegration.Configuration;

namespace SWFC.Infrastructure.M400_Integration.M406_IdentityIntegration.Services;

public sealed class OidcExternalIdentityResolver
{
    private readonly OidcProviderOptions _options;

    public OidcExternalIdentityResolver(IOptions<IdentityIntegrationOptions> options)
    {
        _options = options.Value.Oidc ?? new OidcProviderOptions();
    }

    public OidcExternalIdentity Resolve(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        var subject = GetClaim(principal, _options.ClaimMapping.SubjectClaimType);

        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new InvalidOperationException("OIDC subject claim is missing.");
        }

        var identityKey =
            GetClaim(principal, _options.ClaimMapping.IdentityKeyClaimType) ??
            GetClaim(principal, _options.ClaimMapping.EmailClaimType) ??
            subject;

        if (string.IsNullOrWhiteSpace(identityKey))
        {
            throw new InvalidOperationException("OIDC identity key claim is missing.");
        }

        var displayName =
            GetClaim(principal, _options.ClaimMapping.DisplayNameClaimType) ??
            GetClaim(principal, _options.ClaimMapping.NameClaimType) ??
            GetClaim(principal, _options.ClaimMapping.EmailClaimType) ??
            identityKey;

        var email = GetClaim(principal, _options.ClaimMapping.EmailClaimType);
        var authenticationMethods = GetClaims(principal, _options.ClaimMapping.AuthenticationMethodClaimType);
        var authenticationContext = GetClaim(principal, _options.ClaimMapping.AuthenticationContextClaimType);

        return new OidcExternalIdentity(
            subject.Trim(),
            identityKey.Trim(),
            displayName.Trim(),
            string.IsNullOrWhiteSpace(email) ? null : email.Trim(),
            authenticationMethods,
            string.IsNullOrWhiteSpace(authenticationContext) ? null : authenticationContext.Trim());
    }

    private static string? GetClaim(ClaimsPrincipal principal, string claimType)
    {
        if (string.IsNullOrWhiteSpace(claimType))
        {
            return null;
        }

        return principal.Claims
            .FirstOrDefault(claim => string.Equals(claim.Type, claimType, StringComparison.Ordinal))?
            .Value;
    }

    private static IReadOnlyList<string> GetClaims(ClaimsPrincipal principal, string claimType)
    {
        if (string.IsNullOrWhiteSpace(claimType))
        {
            return Array.Empty<string>();
        }

        return principal.Claims
            .Where(claim => string.Equals(claim.Type, claimType, StringComparison.Ordinal))
            .Select(claim => claim.Value?.Trim())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }
}

public sealed record OidcExternalIdentity(
    string Subject,
    string IdentityKey,
    string DisplayName,
    string? Email,
    IReadOnlyList<string> AuthenticationMethods,
    string? AuthenticationContext);
