using Microsoft.Extensions.DependencyInjection;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Configuration;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Pipeline.Authorization;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Pipeline.Execution;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Pipeline.Validation;

namespace SWFC.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationService, AuthorizationService>();
        services.AddM802PipelineEnforcement();
        services.AddScoped(typeof(IExecutionPipeline<,>), typeof(ExecutionPipeline<,>));
        services.AddScoped(typeof(IPipelineStep<,>), typeof(ValidationStep<,>));
        services.AddScoped(typeof(IPipelineStep<,>), typeof(AuthorizationStep<,>));

        return services;
    }
}