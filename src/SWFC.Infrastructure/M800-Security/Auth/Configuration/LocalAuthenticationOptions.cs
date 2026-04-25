namespace SWFC.Infrastructure.M100_System.M103_Authentication.Configuration;

public sealed class LocalAuthenticationOptions
{
    public string CookieName { get; set; } = "SWFC.Auth";

    public int SessionTimeoutMinutes { get; set; } = 60;

    public int MaxFailedAttempts { get; set; } = 5;

    public int LockoutMinutes { get; set; } = 15;
}

