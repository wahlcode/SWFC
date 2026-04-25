using System.Security.Claims;
using SWFC.Application.M100_System.M103_Authentication;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Infrastructure.Services.Security;

namespace SWFC.Infrastructure.M100_System.M103_Authentication.Services;

public sealed class ClaimsPrincipalFactory
{
    public ClaimsPrincipal Create(
        AuthenticationResultDto result,
        string authenticationScheme)
    {
        var claims = new List<Claim>
        {
            new(SecurityClaimTypes.IdentityKey, result.IdentityKey),
            new(ClaimTypes.NameIdentifier, result.UserId?.ToString() ?? string.Empty),
            new(ClaimTypes.Name, result.Username),
            new(SecurityClaimTypes.DisplayName, result.DisplayName)
        };

        var identity = new ClaimsIdentity(claims, authenticationScheme);

        return new ClaimsPrincipal(identity);
    }

    public ClaimsPrincipal Create(
        SecurityContext securityContext,
        string authenticationScheme)
    {
        var claims = new List<Claim>
        {
            new(SecurityClaimTypes.IdentityKey, securityContext.IdentityKey),
            new(ClaimTypes.NameIdentifier, securityContext.UserId),
            new(ClaimTypes.Name, securityContext.Username),
            new(SecurityClaimTypes.DisplayName, securityContext.DisplayName)
        };

        var identity = new ClaimsIdentity(claims, authenticationScheme);

        return new ClaimsPrincipal(identity);
    }
}

