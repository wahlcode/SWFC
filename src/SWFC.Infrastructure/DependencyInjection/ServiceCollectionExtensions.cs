using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SWFC.Application.Common.Validation;
using SWFC.Application.M200_Business.M201_Assets.Commands;
using SWFC.Application.M200_Business.M201_Assets.Handlers;
using SWFC.Application.M200_Business.M201_Assets.Interfaces;
using SWFC.Application.M200_Business.M201_Assets.Queries;
using SWFC.Application.M200_Business.M201_Assets.Services;
using SWFC.Application.M200_Business.M201_Assets.Validators;
using SWFC.Application.M200_Business.M204_Inventory.Commands;
using SWFC.Application.M200_Business.M204_Inventory.Handlers;
using SWFC.Application.M200_Business.M204_Inventory.Interfaces;
using SWFC.Application.M200_Business.M204_Inventory.Queries;
using SWFC.Application.M200_Business.M204_Inventory.Services;
using SWFC.Application.M200_Business.M204_Inventory.Validators;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Infrastructure.M800_Security.Audit;
using SWFC.Infrastructure.M800_Security.Auth.DependencyInjection;
using SWFC.Infrastructure.Persistence.Context;
using SWFC.Infrastructure.Persistence.Repositories.M200_Business;
using SWFC.Infrastructure.Persistence.Repositories.M800_Security;

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

        services.AddScoped<ICommandValidator<CreateInventoryItemCommand>, CreateInventoryItemValidator>();
        services.AddScoped<ICommandValidator<UpdateInventoryItemCommand>, UpdateInventoryItemValidator>();
        services.AddScoped<ICommandValidator<DeleteInventoryItemCommand>, DeleteInventoryItemValidator>();
        services.AddScoped<ICommandValidator<GetInventoryItemByIdQuery>, GetInventoryItemByIdValidator>();

        services.AddScoped<IAuthorizationPolicy<CreateMachineCommand>, CreateMachinePolicy>();
        services.AddScoped<IAuthorizationPolicy<UpdateMachineCommand>, UpdateMachinePolicy>();
        services.AddScoped<IAuthorizationPolicy<DeleteMachineCommand>, DeleteMachinePolicy>();
        services.AddScoped<IAuthorizationPolicy<GetMachinesQuery>, GetMachinesPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetMachineByIdQuery>, GetMachineByIdPolicy>();

        services.AddScoped<IAuthorizationPolicy<CreateInventoryItemCommand>, CreateInventoryItemPolicy>();
        services.AddScoped<IAuthorizationPolicy<UpdateInventoryItemCommand>, UpdateInventoryItemPolicy>();
        services.AddScoped<IAuthorizationPolicy<DeleteInventoryItemCommand>, DeleteInventoryItemPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetInventoryItemsQuery>, GetInventoryItemsPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetInventoryItemByIdQuery>, GetInventoryItemByIdPolicy>();

        services.AddScoped<IMachineWriteRepository, MachineWriteRepository>();
        services.AddScoped<IMachineReadRepository, MachineReadRepository>();

        services.AddScoped<IInventoryItemWriteRepository, InventoryItemWriteRepository>();
        services.AddScoped<IInventoryItemReadRepository, InventoryItemReadRepository>();
        services.AddScoped<StockReadRepository>();

        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IAuditService, AuditService>();

        services.AddScoped<IUseCaseHandler<CreateMachineCommand, Guid>, CreateMachineHandler>();
        services.AddScoped<IUseCaseHandler<UpdateMachineCommand, bool>, UpdateMachineHandler>();
        services.AddScoped<IUseCaseHandler<DeleteMachineCommand, bool>, DeleteMachineHandler>();
        services.AddScoped<IUseCaseHandler<GetMachinesQuery, IReadOnlyList<MachineListItem>>, GetMachinesHandler>();
        services.AddScoped<IUseCaseHandler<GetMachineByIdQuery, MachineDetailsDto>, GetMachineByIdHandler>();

        services.AddScoped<IUseCaseHandler<CreateInventoryItemCommand, Guid>, CreateInventoryItemHandler>();
        services.AddScoped<IUseCaseHandler<UpdateInventoryItemCommand, bool>, UpdateInventoryItemHandler>();
        services.AddScoped<IUseCaseHandler<DeleteInventoryItemCommand, bool>, DeleteInventoryItemHandler>();
        services.AddScoped<IUseCaseHandler<GetInventoryItemsQuery, IReadOnlyList<InventoryItemListItem>>, GetInventoryItemsHandler>();
        services.AddScoped<IUseCaseHandler<GetInventoryItemByIdQuery, InventoryItemDetailsDto>, GetInventoryItemByIdHandler>();

        services.AddM103Authentication(configuration);

        return services;
    }
}