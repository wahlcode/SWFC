using Microsoft.Extensions.Options;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Infrastructure.M800_Security.Auth.Configuration;

namespace SWFC.Infrastructure.M800_Security.Auth.Providers.Local;

public sealed class LocalCurrentUserService : ICurrentUserService
{
    private readonly AuthenticationOptions _authenticationOptions;

    public LocalCurrentUserService(IOptions<AuthenticationOptions> authenticationOptions)
    {
        _authenticationOptions = authenticationOptions.Value;
    }

    public Task<SecurityContext> GetSecurityContextAsync(CancellationToken cancellationToken = default)
    {
        var local = _authenticationOptions.Local ?? new LocalAuthenticationOptions();

        var securityContext = new SecurityContext(
            userId: local.UserId,
            username: local.UserId,
            isAuthenticated: true,
            roles: local.Roles,
            permissions: local.Permissions);

        return Task.FromResult(securityContext);
    }
}