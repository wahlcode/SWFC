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

        var userId = section["UserId"];
        var isAuthenticated = section.GetValue<bool?>("IsAuthenticated") ?? false;
        var resolvedUserId = string.IsNullOrWhiteSpace(userId) ? "system" : userId.Trim();

        return _securityContextResolver.ResolveAsync(
            identityKey: resolvedUserId,
            fallbackUsername: resolvedUserId,
            isAuthenticated: isAuthenticated,
            cancellationToken: cancellationToken);
    }
}