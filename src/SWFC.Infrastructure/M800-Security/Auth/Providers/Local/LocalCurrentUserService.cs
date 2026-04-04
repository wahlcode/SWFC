using Microsoft.Extensions.Options;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Infrastructure.M800_Security.Auth.Configuration;
using SWFC.Infrastructure.Services.Security;

namespace SWFC.Infrastructure.M800_Security.Auth.Providers.Local;

public sealed class LocalCurrentUserService : ICurrentUserService
{
    private readonly AuthenticationOptions _authenticationOptions;
    private readonly M102SecurityContextResolver _securityContextResolver;

    public LocalCurrentUserService(
        IOptions<AuthenticationOptions> authenticationOptions,
        M102SecurityContextResolver securityContextResolver)
    {
        _authenticationOptions = authenticationOptions.Value;
        _securityContextResolver = securityContextResolver;
    }

    public Task<SecurityContext> GetSecurityContextAsync(CancellationToken cancellationToken = default)
    {
        var local = _authenticationOptions.Local ?? new LocalAuthenticationOptions();

        return _securityContextResolver.ResolveAsync(
            identityKey: local.UserId,
            fallbackUsername: local.UserId,
            isAuthenticated: true,
            cancellationToken: cancellationToken);
    }
}