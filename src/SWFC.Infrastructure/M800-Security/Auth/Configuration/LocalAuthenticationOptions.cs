namespace SWFC.Infrastructure.M800_Security.Auth.Configuration;

public sealed class LocalAuthenticationOptions
{
    public int SessionTimeoutMinutes { get; set; } = 60;
    public int MaxFailedAttempts { get; set; } = 5;
    public int LockoutMinutes { get; set; } = 15;
    public string CookieName { get; set; } = "SWFC.Auth";
}