using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SWFC.Application.Common.Validation;
using SWFC.Application.M100_System.M102_Organization.Authorization;
using SWFC.Application.M100_System.M102_Organization.Commands;
using SWFC.Application.M100_System.M102_Organization.DTOs;
using SWFC.Application.M100_System.M102_Organization.Handlers;
using SWFC.Application.M100_System.M102_Organization.Interfaces;
using SWFC.Application.M100_System.M102_Organization.Queries;
using SWFC.Application.M100_System.M102_Organization.Services;
using SWFC.Application.M100_System.M102_Organization.Validators;
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
using SWFC.Infrastructure.M800_Security.Auth.Configuration;
using SWFC.Infrastructure.M800_Security.Auth.DependencyInjection;
using SWFC.Infrastructure.Persistence.Context;
using SWFC.Infrastructure.Persistence.Repositories.M100_System;
using SWFC.Infrastructure.Persistence.Repositories.M200_Business;
using SWFC.Infrastructure.Persistence.Repositories.M800_Security;
using SWFC.Infrastructure.Services.Security;
using SWFC.Infrastructure.Services.System;

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

        services.Configure<M102InitializationOptions>(options =>
        {
            options.AdminRoleName = "Admin";
            options.DeveloperIdentityKey = "local-admin";
            options.DeveloperDisplayName = "Developer";
            options.CreateRootOrganizationUnit = true;
            options.RootOrganizationUnitCode = "ROOT";
            options.RootOrganizationUnitName = "ROOT";
        });

        services.Configure<AuthenticationOptions>(options =>
        {
            configuration.GetSection(AuthenticationOptions.SectionName).Bind(options);
        });

        services.AddScoped<IM102DataInitializer, M102DataInitializer>();

        services.AddScoped<IUserReadRepository, UserReadRepository>();
        services.AddScoped<IUserWriteRepository, UserWriteRepository>();
        services.AddScoped<IRoleReadRepository, RoleReadRepository>();
        services.AddScoped<IRoleWriteRepository, RoleWriteRepository>();
        services.AddScoped<IOrganizationUnitReadRepository, OrganizationUnitReadRepository>();
        services.AddScoped<IOrganizationUnitWriteRepository, OrganizationUnitWriteRepository>();
        services.AddScoped<IRolePermissionMapper, RolePermissionMapper>();
        services.AddScoped<IM102SecurityProjectionService, M102SecurityProjectionService>();
        services.AddScoped<M102SecurityContextResolver>();

        services.AddScoped<ICommandValidator<CreateUserCommand>, CreateUserValidator>();
        services.AddScoped<ICommandValidator<UpdateUserCommand>, UpdateUserValidator>();
        services.AddScoped<ICommandValidator<CreateRoleCommand>, CreateRoleValidator>();
        services.AddScoped<ICommandValidator<CreateOrganizationUnitCommand>, CreateOrganizationUnitValidator>();
        services.AddScoped<ICommandValidator<GetUserByIdQuery>, GetUserByIdValidator>();
        services.AddScoped<ICommandValidator<AssignRoleToUserCommand>, AssignRoleToUserValidator>();
        services.AddScoped<ICommandValidator<RemoveRoleFromUserCommand>, RemoveRoleFromUserValidator>();
        services.AddScoped<ICommandValidator<AssignOrganizationUnitToUserCommand>, AssignOrganizationUnitToUserValidator>();
        services.AddScoped<ICommandValidator<RemoveOrganizationUnitFromUserCommand>, RemoveOrganizationUnitFromUserValidator>();
        services.AddScoped<ICommandValidator<GetRoleByIdQuery>, GetRoleByIdValidator>();
        services.AddScoped<ICommandValidator<GetOrganizationUnitByIdQuery>, GetOrganizationUnitByIdValidator>();

        services.AddScoped<ICommandValidator<CreateMachineCommand>, CreateMachineValidator>();
        services.AddScoped<ICommandValidator<UpdateMachineCommand>, UpdateMachineValidator>();
        services.AddScoped<ICommandValidator<DeleteMachineCommand>, DeleteMachineValidator>();
        services.AddScoped<ICommandValidator<GetMachineByIdQuery>, GetMachineByIdValidator>();

        services.AddScoped<ICommandValidator<CreateInventoryItemCommand>, CreateInventoryItemValidator>();
        services.AddScoped<ICommandValidator<UpdateInventoryItemCommand>, UpdateInventoryItemValidator>();
        services.AddScoped<ICommandValidator<DeleteInventoryItemCommand>, DeleteInventoryItemValidator>();
        services.AddScoped<ICommandValidator<GetInventoryItemByIdQuery>, GetInventoryItemByIdValidator>();

        services.AddScoped<ICommandValidator<CreateStockMovementCommand>, CreateStockMovementValidator>();
        services.AddScoped<ICommandValidator<GetStockMovementByIdQuery>, GetStockMovementByIdValidator>();

        services.AddScoped<ICommandValidator<CreateStockReservationCommand>, CreateStockReservationValidator>();
        services.AddScoped<ICommandValidator<ReleaseStockReservationCommand>, ReleaseStockReservationValidator>();
        services.AddScoped<ICommandValidator<GetStockReservationByIdQuery>, GetStockReservationByIdValidator>();

        services.AddScoped<ICommandValidator<ConsumeStockReservationCommand>, ConsumeStockReservationValidator>();
        services.AddScoped<ICommandValidator<GetStockAvailabilityQuery>, GetStockAvailabilityValidator>();

        services.AddScoped<IAuthorizationPolicy<CreateUserCommand>, CreateUserPolicy>();
        services.AddScoped<IAuthorizationPolicy<UpdateUserCommand>, UpdateUserPolicy>();
        services.AddScoped<IAuthorizationPolicy<CreateRoleCommand>, CreateRolePolicy>();
        services.AddScoped<IAuthorizationPolicy<CreateOrganizationUnitCommand>, CreateOrganizationUnitPolicy>();
        services.AddScoped<IAuthorizationPolicy<AssignRoleToUserCommand>, AssignRoleToUserPolicy>();
        services.AddScoped<IAuthorizationPolicy<RemoveRoleFromUserCommand>, RemoveRoleFromUserPolicy>();
        services.AddScoped<IAuthorizationPolicy<AssignOrganizationUnitToUserCommand>, AssignOrganizationUnitToUserPolicy>();
        services.AddScoped<IAuthorizationPolicy<RemoveOrganizationUnitFromUserCommand>, RemoveOrganizationUnitFromUserPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetUsersQuery>, GetUsersPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetUserByIdQuery>, GetUserByIdPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetRolesQuery>, GetRolesPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetRoleByIdQuery>, GetRoleByIdPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetOrganizationUnitsQuery>, GetOrganizationUnitsPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetOrganizationUnitByIdQuery>, GetOrganizationUnitByIdPolicy>();

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

        services.AddScoped<IAuthorizationPolicy<CreateStockMovementCommand>, CreateStockMovementPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetStockMovementsQuery>, GetStockMovementsPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetStockMovementByIdQuery>, GetStockMovementByIdPolicy>();

        services.AddScoped<IAuthorizationPolicy<CreateStockReservationCommand>, CreateStockReservationPolicy>();
        services.AddScoped<IAuthorizationPolicy<ReleaseStockReservationCommand>, ReleaseStockReservationPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetStockReservationsQuery>, GetStockReservationsPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetStockReservationByIdQuery>, GetStockReservationByIdPolicy>();

        services.AddScoped<IAuthorizationPolicy<ConsumeStockReservationCommand>, ConsumeStockReservationPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetStockAvailabilityQuery>, GetStockAvailabilityPolicy>();

        services.AddScoped<IMachineWriteRepository, MachineWriteRepository>();
        services.AddScoped<IMachineReadRepository, MachineReadRepository>();

        services.AddScoped<IInventoryItemWriteRepository, InventoryItemWriteRepository>();
        services.AddScoped<IInventoryItemReadRepository, InventoryItemReadRepository>();

        services.AddScoped<IStockMovementReadRepository, StockMovementReadRepository>();
        services.AddScoped<IStockMovementWriteRepository, StockMovementWriteRepository>();
        services.AddScoped<StockReadRepository>();

        services.AddScoped<IStockReservationReadRepository, StockReservationReadRepository>();
        services.AddScoped<IStockReservationWriteRepository, StockReservationWriteRepository>();

        services.AddScoped<IInventoryAvailabilityCalculator, InventoryAvailabilityCalculator>();

        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IAuditService, AuditService>();

        services.AddScoped<IUseCaseHandler<CreateUserCommand, Guid>, CreateUserHandler>();
        services.AddScoped<IUseCaseHandler<UpdateUserCommand, bool>, UpdateUserHandler>();
        services.AddScoped<IUseCaseHandler<CreateRoleCommand, Guid>, CreateRoleHandler>();
        services.AddScoped<IUseCaseHandler<CreateOrganizationUnitCommand, Guid>, CreateOrganizationUnitHandler>();
        services.AddScoped<IUseCaseHandler<AssignRoleToUserCommand, bool>, AssignRoleToUserHandler>();
        services.AddScoped<IUseCaseHandler<RemoveRoleFromUserCommand, bool>, RemoveRoleFromUserHandler>();
        services.AddScoped<IUseCaseHandler<AssignOrganizationUnitToUserCommand, bool>, AssignOrganizationUnitToUserHandler>();
        services.AddScoped<IUseCaseHandler<RemoveOrganizationUnitFromUserCommand, bool>, RemoveOrganizationUnitFromUserHandler>();
        services.AddScoped<IUseCaseHandler<GetUsersQuery, IReadOnlyList<UserListItem>>, GetUsersHandler>();
        services.AddScoped<IUseCaseHandler<GetUserByIdQuery, UserDetailsDto>, GetUserByIdHandler>();
        services.AddScoped<IUseCaseHandler<GetRolesQuery, IReadOnlyList<RoleListItem>>, GetRolesHandler>();
        services.AddScoped<IUseCaseHandler<GetRoleByIdQuery, RoleDetailsDto>, GetRoleByIdHandler>();
        services.AddScoped<IUseCaseHandler<GetOrganizationUnitsQuery, IReadOnlyList<OrganizationUnitListItem>>, GetOrganizationUnitsHandler>();
        services.AddScoped<IUseCaseHandler<GetOrganizationUnitByIdQuery, OrganizationUnitDetailsDto>, GetOrganizationUnitByIdHandler>();

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

        services.AddScoped<IUseCaseHandler<CreateStockMovementCommand, Guid>, CreateStockMovementHandler>();
        services.AddScoped<IUseCaseHandler<GetStockMovementsQuery, IReadOnlyList<StockMovementListItem>>, GetStockMovementsHandler>();
        services.AddScoped<IUseCaseHandler<GetStockMovementByIdQuery, StockMovementDetailsDto>, GetStockMovementByIdHandler>();

        services.AddScoped<IUseCaseHandler<CreateStockReservationCommand, Guid>, CreateStockReservationHandler>();
        services.AddScoped<IUseCaseHandler<ReleaseStockReservationCommand, bool>, ReleaseStockReservationHandler>();
        services.AddScoped<IUseCaseHandler<GetStockReservationsQuery, IReadOnlyList<StockReservationListItem>>, GetStockReservationsHandler>();
        services.AddScoped<IUseCaseHandler<GetStockReservationByIdQuery, StockReservationDetailsDto>, GetStockReservationByIdHandler>();

        services.AddScoped<IUseCaseHandler<ConsumeStockReservationCommand, Guid>, ConsumeStockReservationHandler>();
        services.AddScoped<IUseCaseHandler<GetStockAvailabilityQuery, StockAvailabilityDto>, GetStockAvailabilityHandler>();

        services.AddM103Authentication(configuration);

        return services;
    }
}