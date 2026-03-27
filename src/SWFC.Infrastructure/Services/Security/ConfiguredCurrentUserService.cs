using Microsoft.Extensions.Configuration;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;

namespace SWFC.Infrastructure.Services.Security;

public sealed class ConfiguredCurrentUserService : ICurrentUserService
{
    private const string SectionName = "Security:CurrentUser";
    private readonly IConfiguration _configuration;

    public ConfiguredCurrentUserService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<SecurityContext> GetSecurityContextAsync(CancellationToken cancellationToken = default)
    {
        var section = _configuration.GetSection(SectionName);

        var userId = section["UserId"];
        var isAuthenticated = section.GetValue<bool?>("IsAuthenticated") ?? false;
        var roles = section.GetSection("Roles").Get<string[]>() ?? Array.Empty<string>();
        var permissions = section.GetSection("Permissions").Get<string[]>() ?? Array.Empty<string>();

        var securityContext = new SecurityContext(
            userId: string.IsNullOrWhiteSpace(userId) ? "system" : userId,
            isAuthenticated: isAuthenticated,
            roles: roles,
            permissions: permissions);

        return Task.FromResult(securityContext);
    }
}
