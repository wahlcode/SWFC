using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SWFC.Application.Common.Validation;
using SWFC.Application.M100_System.M103_Authentication.Commands;
using SWFC.Application.M100_System.M103_Authentication.DTOs;
using SWFC.Application.M100_System.M103_Authentication.Handlers;
using SWFC.Application.M100_System.M103_Authentication.Interfaces;
using SWFC.Application.M100_System.M103_Authentication.Validators;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Infrastructure.M800_Security.Auth.Configuration;
using SWFC.Infrastructure.M800_Security.Auth.Providers.Local;
using SWFC.Infrastructure.M800_Security.Auth.Services;
using SWFC.Infrastructure.Persistence.Repositories.M103_Authentication;

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
            return services;
        }

        throw new InvalidOperationException(
            $"Unsupported authentication mode '{options.Mode}'. Allowed values: '{AuthenticationModes.Local}', '{AuthenticationModes.Sso}'.");
    }
}