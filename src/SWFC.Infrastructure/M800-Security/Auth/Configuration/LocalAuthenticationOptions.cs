namespace SWFC.Infrastructure.M800_Security.Auth.Configuration;

public sealed class LocalAuthenticationOptions
{
    public string UserId { get; set; } = "dev-local-user";

    public string[] Roles { get; set; } = Array.Empty<string>();

    public string[] Permissions { get; set; } = Array.Empty<string>();
}