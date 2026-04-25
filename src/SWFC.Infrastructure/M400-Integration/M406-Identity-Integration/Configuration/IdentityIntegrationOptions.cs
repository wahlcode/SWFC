namespace SWFC.Infrastructure.M400_Integration.M406_IdentityIntegration.Configuration;

public sealed class IdentityIntegrationOptions
{
    public const string SectionName = "IdentityIntegration";

    public OidcProviderOptions Oidc { get; set; } = new();
}

public sealed class OidcProviderOptions
{
    public bool Enabled { get; set; }

    public string DisplayName { get; set; } = "Single Sign-On";

    public string Authority { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;

    public string? ClientSecret { get; set; }

    public string CallbackPath { get; set; } = "/auth/oidc/callback";

    public string SignedOutCallbackPath { get; set; } = "/auth/oidc/signout-callback";

    public string PostLogoutRedirectPath { get; set; } = "/auth/login";

    public string[] Scopes { get; set; } = ["openid", "profile", "email"];

    public OidcClaimMappingOptions ClaimMapping { get; set; } = new();

    public bool IsConfigured()
    {
        return Enabled &&
               !string.IsNullOrWhiteSpace(Authority) &&
               !string.IsNullOrWhiteSpace(ClientId);
    }
}

public sealed class OidcClaimMappingOptions
{
    public string SubjectClaimType { get; set; } = "sub";

    public string EmailClaimType { get; set; } = "email";

    public string NameClaimType { get; set; } = "name";

    public string IdentityKeyClaimType { get; set; } = "email";

    public string DisplayNameClaimType { get; set; } = "name";

    public string AuthenticationMethodClaimType { get; set; } = "amr";

    public string AuthenticationContextClaimType { get; set; } = "acr";
}
