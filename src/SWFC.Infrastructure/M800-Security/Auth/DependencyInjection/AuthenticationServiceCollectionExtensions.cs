using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Infrastructure.M800_Security.Auth.Configuration;
using SWFC.Infrastructure.M800_Security.Auth.Providers.Local;

namespace SWFC.Infrastructure.M800_Security.Auth.DependencyInjection;

public static class AuthenticationServiceCollectionExtensions
{
    public static IServiceCollection AddM103Authentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AuthenticationOptions>(options =>
        {
            configuration.GetSection(AuthenticationOptions.SectionName).Bind(options);
        });

        var options = configuration
            .GetSection(AuthenticationOptions.SectionName)
            .Get<AuthenticationOptions>() ?? new AuthenticationOptions();

        if (string.Equals(options.Mode, AuthenticationModes.Local, StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<ICurrentUserService, LocalCurrentUserService>();
            return services;
        }

        if (string.Equals(options.Mode, AuthenticationModes.Sso, StringComparison.OrdinalIgnoreCase))
        {
            return services;
        }

        throw new InvalidOperationException(
            $"Unsupported authentication mode '{options.Mode}'. Allowed values: '{AuthenticationModes.Local}', '{AuthenticationModes.Sso}'.");
    }
}