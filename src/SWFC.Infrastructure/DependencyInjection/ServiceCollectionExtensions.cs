using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SWFC.Application.Common.Validation;
using SWFC.Application.M200_Business.M201_Assets.Commands;
using SWFC.Application.M200_Business.M201_Assets.Handlers;
using SWFC.Application.M200_Business.M201_Assets.Interfaces;
using SWFC.Application.M200_Business.M201_Assets.Services;
using SWFC.Application.M200_Business.M201_Assets.Validators;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Infrastructure.Persistence.Context;
using SWFC.Infrastructure.Persistence.Repositories.M200_Business;
using SWFC.Infrastructure.Services.Security;

namespace SWFC.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=swfc";

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ICurrentUserService, ConfiguredCurrentUserService>();

        services.AddScoped<ICommandValidator<CreateMachineCommand>, CreateMachineValidator>();
        services.AddScoped<IAuthorizationPolicy<CreateMachineCommand>, CreateMachinePolicy>();
        services.AddScoped<IMachineWriteRepository, MachineWriteRepository>();

        
        services.AddScoped<IUseCaseHandler<CreateMachineCommand, Guid>, CreateMachineHandler>();

        return services;
    }
}
