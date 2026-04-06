using System.Security.Claims;
using SWFC.Application.M100_System.M103_Authentication.DTOs;

namespace SWFC.Infrastructure.M800_Security.Auth.Services;

public sealed class ClaimsPrincipalFactory
{
    public ClaimsPrincipal Create(AuthenticationResultDto authenticationResult, string authenticationScheme)
    {
        if (!authenticationResult.Succeeded || authenticationResult.UserId is null)
        {
            throw new InvalidOperationException("Cannot create claims principal for failed authentication.");
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, authenticationResult.UserId.Value.ToString()),
            new("identity_key", authenticationResult.IdentityKey),
            new(ClaimTypes.Name, authenticationResult.Username),
            new("display_name", authenticationResult.DisplayName)
        };

        var identity = new ClaimsIdentity(claims, authenticationScheme);
        return new ClaimsPrincipal(identity);
    }
}