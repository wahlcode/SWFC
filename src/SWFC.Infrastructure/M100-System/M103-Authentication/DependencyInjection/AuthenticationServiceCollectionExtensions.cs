using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SWFC.Application.M100_System.M103_Authentication;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Infrastructure.M100_System.M103_Authentication.Configuration;
using SWFC.Infrastructure.M100_System.M103_Authentication.Providers.Local;
using SWFC.Infrastructure.M100_System.M103_Authentication.Providers.Sso;
using SWFC.Infrastructure.M100_System.M103_Authentication.Services;
using SWFC.Infrastructure.Persistence.Repositories.M103_Authentication;

namespace SWFC.Infrastructure.M100_System.M103_Authentication.DependencyInjection;

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

        services.AddScoped<ILocalCredentialReadRepository, LocalCredentialReadRepository>();
        services.AddScoped<ILocalCredentialWriteRepository, LocalCredentialWriteRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ILocalAuthenticationService, LocalAuthenticationService>();
        services.AddScoped<ClaimsPrincipalFactory>();

        services.AddScoped<ICommandValidator<AuthenticateLocalUserCommand>, AuthenticateLocalUserValidator>();
        services.AddScoped<ICommandValidator<ChangeOwnPasswordCommand>, ChangeOwnPasswordValidator>();
        services.AddScoped<ICommandValidator<AdminSetUserPasswordCommand>, AdminSetUserPasswordValidator>();

        services.AddScoped<IUseCaseHandler<AuthenticateLocalUserCommand, AuthenticationResultDto>, AuthenticateLocalUserHandler>();
        services.AddScoped<IUseCaseHandler<ChangeOwnPasswordCommand, bool>, ChangeOwnPasswordHandler>();
        services.AddScoped<IUseCaseHandler<AdminSetUserPasswordCommand, bool>, AdminSetUserPasswordHandler>();

        services.AddHttpContextAccessor();

        if (string.Equals(options.Mode, AuthenticationModes.Local, StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<ICurrentUserService, LocalCurrentUserService>();
            return services;
        }

        if (string.Equals(options.Mode, AuthenticationModes.Sso, StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<ICurrentUserService, SsoCurrentUserService>();
            return services;
        }

        throw new InvalidOperationException(
            $"Unsupported authentication mode '{options.Mode}'. Allowed values: '{AuthenticationModes.Local}', '{AuthenticationModes.Sso}'.");
    }
}
