using Microsoft.Extensions.DependencyInjection;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Pipeline.Authorization;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Pipeline.Execution;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Pipeline.Validation;

namespace SWFC.Application.M800_Security.M802_ApplicationSecurity.Configuration;

public static class PipelineServiceCollectionExtensions
{
    public static IServiceCollection AddM802PipelineEnforcement(this IServiceCollection services)
    {
        services.AddScoped(typeof(IExecutionPipeline<,>), typeof(ExecutionPipeline<,>));

        services.AddScoped(typeof(IPipelineStep<,>), typeof(ValidationStep<,>));
        services.AddScoped(typeof(IPipelineStep<,>), typeof(AuthorizationStep<,>));

        return services;
    }
}

