namespace SWFC.Infrastructure.M100_System.M103_Authentication.Configuration;

public sealed class AuthenticationOptions
{
    public const string SectionName = "Authentication";

    public string Mode { get; set; } = AuthenticationModes.Local;

    public LocalAuthenticationOptions Local { get; set; } = new();

    public LegacyDeveloperOptions LegacyDeveloper { get; set; } = new();
    public InitialSuperAdminOptions InitialSuperAdmin { get; set; } = new();
}
