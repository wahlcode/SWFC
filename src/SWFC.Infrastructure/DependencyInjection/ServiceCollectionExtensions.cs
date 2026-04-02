using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Application.Common.Validation;
using SWFC.Application.M200_Business.M201_Assets.Commands;
using SWFC.Application.M200_Business.M201_Assets.Handlers;
using SWFC.Application.M200_Business.M201_Assets.Interfaces;
using SWFC.Application.M200_Business.M201_Assets.Queries;
using SWFC.Application.M200_Business.M201_Assets.Services;
using SWFC.Application.M200_Business.M201_Assets.Validators;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Infrastructure.M800_Security.Audit;
using SWFC.Infrastructure.M800_Security.Auth.DependencyInjection;
using SWFC.Infrastructure.Persistence.Context;
using SWFC.Infrastructure.Persistence.Repositories.M800_Security;
using SWFC.Infrastructure.Persistence.Repositories.M200_Business;

namespace SWFC.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is missing.");
        }

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ICommandValidator<CreateMachineCommand>, CreateMachineValidator>();
        services.AddScoped<ICommandValidator<UpdateMachineCommand>, UpdateMachineValidator>();
        services.AddScoped<ICommandValidator<DeleteMachineCommand>, DeleteMachineValidator>();
        services.AddScoped<ICommandValidator<GetMachineByIdQuery>, GetMachineByIdValidator>();

        services.AddScoped<IAuthorizationPolicy<CreateMachineCommand>, CreateMachinePolicy>();
        services.AddScoped<IAuthorizationPolicy<UpdateMachineCommand>, UpdateMachinePolicy>();
        services.AddScoped<IAuthorizationPolicy<DeleteMachineCommand>, DeleteMachinePolicy>();
        services.AddScoped<IAuthorizationPolicy<GetMachinesQuery>, GetMachinesPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetMachineByIdQuery>, GetMachineByIdPolicy>();

        services.AddScoped<IMachineWriteRepository, MachineWriteRepository>();
        services.AddScoped<IMachineReadRepository, MachineReadRepository>();

        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IAuditService, AuditService>();

        services.AddScoped<IUseCaseHandler<CreateMachineCommand, Guid>, CreateMachineHandler>();
        services.AddScoped<IUseCaseHandler<UpdateMachineCommand, bool>, UpdateMachineHandler>();
        services.AddScoped<IUseCaseHandler<DeleteMachineCommand, bool>, DeleteMachineHandler>();
        services.AddScoped<IUseCaseHandler<GetMachinesQuery, IReadOnlyList<MachineListItem>>, GetMachinesHandler>();
        services.AddScoped<IUseCaseHandler<GetMachineByIdQuery, MachineDetailsDto>, GetMachineByIdHandler>();

        services.AddM103Authentication(configuration);

        return services;
    }
}