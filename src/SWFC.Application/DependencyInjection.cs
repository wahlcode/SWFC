using Microsoft.Extensions.DependencyInjection;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Configuration;
using SWFC.Application.M800_Security.M806_AccessControl.Decisions;

namespace SWFC.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationService, AuthorizationService>();
        services.AddScoped<IAccessDecisionService, AccessDecisionService>();
        services.AddM802PipelineEnforcement();

        return services;
    }
}

