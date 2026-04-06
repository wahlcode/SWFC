namespace SWFC.Infrastructure.M800_Security.Auth.Configuration;

public sealed class InitialAdminOptions
{
    public string Username { get; set; } = "local-admin";
    public string Password { get; set; } = string.Empty;
}