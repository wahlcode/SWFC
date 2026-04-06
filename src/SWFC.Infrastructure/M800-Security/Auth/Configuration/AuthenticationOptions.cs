namespace SWFC.Infrastructure.M800_Security.Auth.Configuration;

public sealed class AuthenticationOptions
{
    public const string SectionName = "Authentication";

    public string Mode { get; set; } = "Local";

    public LocalAuthenticationOptions Local { get; set; } = new();

    public InitialAdminOptions InitialAdmin { get; set; } = new();
}