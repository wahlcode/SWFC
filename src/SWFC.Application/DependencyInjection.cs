using Microsoft.Extensions.DependencyInjection;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Configuration;

namespace SWFC.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddM802PipelineEnforcement();
        return services;
    }
}
