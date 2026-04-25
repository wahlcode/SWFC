using Microsoft.Extensions.Configuration;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;

namespace SWFC.Infrastructure.Services.Security;

public sealed class ConfiguredCurrentUserService : ICurrentUserService
{
    private const string SectionName = "Security:CurrentUser";
    private readonly IConfiguration _configuration;
    private readonly M102SecurityContextResolver _securityContextResolver;

    public ConfiguredCurrentUserService(
        IConfiguration configuration,
        M102SecurityContextResolver securityContextResolver)
    {
        _configuration = configuration;
        _securityContextResolver = securityContextResolver;
    }

    public Task<SecurityContext> GetSecurityContextAsync(CancellationToken cancellationToken = default)
    {
        var section = _configuration.GetSection(SectionName);

        var configuredUserId = section["UserId"];
        var configuredIdentityKey = section["IdentityKey"];
        var configuredUsername = section["Username"];
        var isAuthenticated = section.GetValue<bool?>("IsAuthenticated") ?? false;
        var isDeveloperMode = section.GetValue<bool?>("IsDeveloperMode") ?? false;

        var resolvedUserId = string.IsNullOrWhiteSpace(configuredUserId)
            ? string.Empty
            : configuredUserId.Trim();

        var resolvedIdentityKey = string.IsNullOrWhiteSpace(configuredIdentityKey)
            ? resolvedUserId
            : configuredIdentityKey.Trim();

        var resolvedUsername = string.IsNullOrWhiteSpace(configuredUsername)
            ? resolvedIdentityKey
            : configuredUsername.Trim();

        return _securityContextResolver.ResolveAsync(
            userId: resolvedUserId,
            identityKey: resolvedIdentityKey,
            fallbackUsername: resolvedUsername,
            isAuthenticated: isAuthenticated,
            isDeveloperMode: isDeveloperMode,
            cancellationToken: cancellationToken);
    }
}

