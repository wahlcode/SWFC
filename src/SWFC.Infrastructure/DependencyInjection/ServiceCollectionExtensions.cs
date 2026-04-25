using SWFC.Application.M100_System.M102_Organization.Users.Delegations;
using SWFC.Application.M100_System.M102_Organization.Users.ExternalPersons;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SWFC.Application.M100_System.M102_Organization.Assignments;
using SWFC.Application.M100_System.M102_Organization.CostCenters;
using SWFC.Application.M100_System.M102_Organization.OrganizationUnits;
using SWFC.Application.M100_System.M102_Organization.ShiftModels;
using SWFC.Application.M100_System.M102_Organization.Users;
using SWFC.Application.M100_System.M103_Authentication;
using SWFC.Application.M200_Business.M201_Assets.MachineComponentAreas;
using SWFC.Application.M200_Business.M201_Assets.MachineComponents;
using SWFC.Application.M200_Business.M201_Assets.Machines;
using SWFC.Application.M200_Business.M202_Maintenance.MaintenanceOrders;
using SWFC.Application.M200_Business.M202_Maintenance.MaintenancePlans;
using SWFC.Application.M200_Business.M204_Inventory.Items;
using SWFC.Application.M200_Business.M204_Inventory.Locations;
using SWFC.Application.M200_Business.M204_Inventory.Reservations;
using SWFC.Application.M200_Business.M204_Inventory.Shared;
using SWFC.Application.M200_Business.M204_Inventory.Stock;
using SWFC.Application.M200_Business.M205_Energy.Analysis;
using SWFC.Application.M200_Business.M205_Energy.EnergyMeters;
using SWFC.Application.M200_Business.M205_Energy.EnergyReadings;
using SWFC.Application.M700_Support.M701_BugTracking;
using SWFC.Application.M700_Support.M702_ChangeRequests;
using SWFC.Application.M700_Support.M703_SupportCases;
using SWFC.Application.M700_Support.M704_Incident_Management;
using SWFC.Application.M700_Support.M705_Knowledge_Base;
using SWFC.Application.M700_Support.M706_SLA_Service_Levels;
using SWFC.Application.M800_Security.M801_Access;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Configuration;
using SWFC.Application.M800_Security.M803_Visibility;
using SWFC.Application.M800_Security.M803_Visibility.AccessRules;
using SWFC.Application.M800_Security.M805_AuditCompliance.Entries;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Application.M800_Security.M806_AccessControl.Assignments;
using SWFC.Application.M800_Security.M806_AccessControl.Permissions;
using SWFC.Application.M800_Security.M806_AccessControl.RolePermissions;
using SWFC.Application.M800_Security.M806_AccessControl.Roles;
using SWFC.Application.M800_Security.M806_AccessControl.Shared;
using SWFC.Application.M800_Security.M806_AccessControl.Users;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Infrastructure.M400_Integration.M406_IdentityIntegration.Configuration;
using SWFC.Infrastructure.M400_Integration.M406_IdentityIntegration.Services;
using SWFC.Infrastructure.M100_System.M103_Authentication.DependencyInjection;
using SWFC.Infrastructure.M100_System.M103_Authentication.Services;
using SWFC.Infrastructure.M100_System.M107_SetupDeployment;
using SWFC.Infrastructure.M800_Security.Audit;
using SWFC.Infrastructure.Persistence.Context;
using SWFC.Infrastructure.Persistence.Repositories.M100_System;
using SWFC.Infrastructure.Persistence.Repositories.M200_Business;
using SWFC.Infrastructure.Persistence.Repositories.M200_Business.M205_Energy;
using SWFC.Infrastructure.Persistence.Repositories.M700_Support;
using SWFC.Infrastructure.Persistence.Repositories.M800_Security;
using SWFC.Infrastructure.Services.Security;
using SWFC.Infrastructure.Services.System;
using SWFC.Application.M200_Business.M206_Purchasing.GoodsReceipts;
using SWFC.Application.M200_Business.M206_Purchasing.PurchaseOrders;
using SWFC.Application.M200_Business.M206_Purchasing.PurchaseRequirements;
using SWFC.Application.M200_Business.M206_Purchasing.RequestForQuotations;
using SWFC.Application.M200_Business.M206_Purchasing.Suppliers;
using SWFC.Infrastructure.Persistence.Repositories.M200_Business.M206_Purchasing;

namespace SWFC.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        var configuration = new ConfigurationBuilder().Build();
        return services.AddInfrastructure(configuration);
    }

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.Configure<M107SetupOptions>(options =>
        {
            configuration.GetSection(M107SetupOptions.SectionName).Bind(options);
        });

        services.Configure<IdentityIntegrationOptions>(options =>
        {
            configuration.GetSection(IdentityIntegrationOptions.SectionName).Bind(options);
        });

        services.AddM103Authentication(configuration);

        services.AddScoped<IUserReadRepository, UserReadRepository>();
        services.AddScoped<IUserWriteRepository, UserWriteRepository>();
        services.AddScoped<IUserHistoryReadRepository, UserHistoryReadRepository>();
        services.AddScoped<IUserHistoryWriteRepository, UserHistoryWriteRepository>();
        services.AddScoped<IRoleReadRepository, RoleReadRepository>();
        services.AddScoped<IRoleWriteRepository, RoleWriteRepository>();
        services.AddScoped<IPermissionReadRepository, PermissionReadRepository>();
        services.AddScoped<IRolePermissionReadRepository, RolePermissionReadRepository>();
        services.AddScoped<IRolePermissionWriteRepository, RolePermissionWriteRepository>();
        services.AddScoped<IUserRoleReadRepository, UserRoleReadRepository>();
        services.AddScoped<IOrganizationUnitReadRepository, OrganizationUnitReadRepository>();
        services.AddScoped<IOrganizationUnitWriteRepository, OrganizationUnitWriteRepository>();
        services.AddScoped<IUserOrganizationAssignmentWriteRepository, UserOrganizationAssignmentWriteRepository>();
        services.AddScoped<IUserRoleAssignmentWriteRepository, UserRoleAssignmentWriteRepository>();

        services.AddScoped<ICostCenterReadRepository, CostCenterReadRepository>();
        services.AddScoped<ICostCenterWriteRepository, CostCenterWriteRepository>();

        services.AddScoped<IShiftModelReadRepository, ShiftModelReadRepository>();
        services.AddScoped<IShiftModelWriteRepository, ShiftModelWriteRepository>();

        services.AddScoped<IMachineReadRepository, MachineReadRepository>();
        services.AddScoped<IMachineWriteRepository, MachineWriteRepository>();
        services.AddScoped<IMachineComponentAreaReadRepository, MachineComponentAreaReadRepository>();
        services.AddScoped<IMachineComponentAreaWriteRepository, MachineComponentAreaWriteRepository>();
        services.AddScoped<IMachineComponentReadRepository, MachineComponentReadRepository>();
        services.AddScoped<IMachineComponentWriteRepository, MachineComponentWriteRepository>();

        services.AddScoped<IMaintenanceOrderReadRepository, MaintenanceOrderReadRepository>();
        services.AddScoped<IMaintenanceOrderWriteRepository, MaintenanceOrderWriteRepository>();
        services.AddScoped<IMaintenancePlanReadRepository, MaintenancePlanReadRepository>();
        services.AddScoped<IMaintenancePlanWriteRepository, MaintenancePlanWriteRepository>();

        services.AddScoped<IInventoryItemReadRepository, InventoryItemReadRepository>();
        services.AddScoped<IInventoryItemWriteRepository, InventoryItemWriteRepository>();
        services.AddScoped<ILocationReadRepository, LocationReadRepository>();
        services.AddScoped<ILocationWriteRepository, LocationWriteRepository>();
        services.AddScoped<IStockReadRepository, StockReadRepository>();
        services.AddScoped<IStockWriteRepository, StockWriteRepository>();
        services.AddScoped<IStockMovementReadRepository, StockMovementReadRepository>();
        services.AddScoped<IStockMovementWriteRepository, StockMovementWriteRepository>();
        services.AddScoped<IStockReservationReadRepository, StockReservationReadRepository>();
        services.AddScoped<IStockReservationWriteRepository, StockReservationWriteRepository>();
        services.AddScoped<IInventoryAvailabilityCalculator, StockAvailabilityCalculator>();

        services.AddScoped<IEnergyMeterReadRepository, EnergyMeterReadRepository>();
        services.AddScoped<IEnergyMeterWriteRepository, EnergyMeterWriteRepository>();
        services.AddScoped<IEnergyReadingReadRepository, EnergyReadingReadRepository>();
        services.AddScoped<IEnergyReadingWriteRepository, EnergyReadingWriteRepository>();
        services.AddScoped<IPurchaseRequirementReadRepository, PurchaseRequirementReadRepository>();
        services.AddScoped<IPurchaseRequirementWriteRepository, PurchaseRequirementWriteRepository>();
        services.AddScoped<ISupplierReadRepository, SupplierReadRepository>();
        services.AddScoped<ISupplierWriteRepository, SupplierWriteRepository>();
        services.AddScoped<IPurchaseOrderReadRepository, PurchaseOrderReadRepository>();
        services.AddScoped<IPurchaseOrderWriteRepository, PurchaseOrderWriteRepository>();
        services.AddScoped<IGoodsReceiptReadRepository, GoodsReceiptReadRepository>();
        services.AddScoped<IGoodsReceiptWriteRepository, GoodsReceiptWriteRepository>();
        services.AddScoped<IRequestForQuotationReadRepository, RequestForQuotationReadRepository>();
        services.AddScoped<IRequestForQuotationWriteRepository, RequestForQuotationWriteRepository>();
        services.AddScoped<IBugReadRepository, BugReadRepository>();
        services.AddScoped<IBugWriteRepository, BugWriteRepository>();
        services.AddScoped<IChangeRequestReadRepository, ChangeRequestReadRepository>();
        services.AddScoped<IChangeRequestWriteRepository, ChangeRequestWriteRepository>();
        services.AddScoped<ISupportCaseReadRepository, SupportCaseReadRepository>();
        services.AddScoped<ISupportCaseWriteRepository, SupportCaseWriteRepository>();
        services.AddScoped<IIncidentReadRepository, IncidentReadRepository>();
        services.AddScoped<IIncidentWriteRepository, IncidentWriteRepository>();
        services.AddScoped<IKnowledgeEntryReadRepository, KnowledgeEntryReadRepository>();
        services.AddScoped<IKnowledgeEntryWriteRepository, KnowledgeEntryWriteRepository>();
        services.AddScoped<IServiceLevelReadRepository, ServiceLevelReadRepository>();
        services.AddScoped<IServiceLevelWriteRepository, ServiceLevelWriteRepository>();

        services.AddScoped<IAccessRuleReadRepository, AccessRuleReadRepository>();
        services.AddScoped<IAccessRuleWriteRepository, AccessRuleWriteRepository>();
        services.AddScoped<IVisibilityResolver, VisibilityResolver>();
        services.AddScoped<IVisibilityTargetContextRepository, VisibilityTargetContextRepository>();

        services.AddScoped<M102SecurityContextResolver>();
        services.AddScoped<IM102SecurityProjectionService, M102SecurityProjectionService>();
        services.AddScoped<ISecurityAdministrationGuard, SecurityAdministrationGuard>();
        services.AddScoped<IM107SetupInitializer, M107SetupInitializer>();
        services.AddScoped<OidcExternalIdentityResolver>();
        services.AddScoped<OidcAuthenticationFlowService>();

        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IAuditService, AuditService>();

        services.AddScoped<ICommandValidator<CreateUserCommand>, CreateUserValidator>();
        services.AddScoped<ICommandValidator<UpdateUserCommand>, UpdateUserValidator>();
        services.AddScoped<ICommandValidator<ChangeUserStatusCommand>, ChangeUserStatusValidator>();
        services.AddScoped<ICommandValidator<CreateRoleCommand>, CreateRoleValidator>();
        services.AddScoped<ICommandValidator<UpdateRoleCommand>, UpdateRoleValidator>();
        services.AddScoped<ICommandValidator<GetPermissionsQuery>, GetPermissionsValidator>();
        services.AddScoped<ICommandValidator<GetRolePermissionsQuery>, GetRolePermissionsValidator>();
        services.AddScoped<ICommandValidator<GetSecurityUserByIdQuery>, GetSecurityUserByIdValidator>();
        services.AddScoped<ICommandValidator<SetRolePermissionsCommand>, SetRolePermissionsValidator>();
        services.AddScoped<ICommandValidator<CreateOrganizationUnitCommand>, CreateOrganizationUnitValidator>();
        services.AddScoped<ICommandValidator<UpdateOrganizationUnitCommand>, UpdateOrganizationUnitValidator>();
        services.AddScoped<ICommandValidator<AssignRoleToUserCommand>, AssignRoleToUserValidator>();
        services.AddScoped<ICommandValidator<RemoveRoleFromUserCommand>, RemoveRoleFromUserValidator>();
        services.AddScoped<ICommandValidator<AssignOrganizationUnitToUserCommand>, AssignOrganizationUnitToUserValidator>();
        services.AddScoped<ICommandValidator<RemoveOrganizationUnitFromUserCommand>, RemoveOrganizationUnitFromUserValidator>();

        services.AddScoped<ICommandValidator<CreateCostCenterCommand>, CreateCostCenterValidator>();
        services.AddScoped<ICommandValidator<UpdateCostCenterCommand>, UpdateCostCenterValidator>();
        services.AddScoped<ICommandValidator<GetCostCenterByIdQuery>, GetCostCenterByIdValidator>();

        services.AddScoped<ICommandValidator<CreateShiftModelCommand>, CreateShiftModelValidator>();
        services.AddScoped<ICommandValidator<UpdateShiftModelCommand>, UpdateShiftModelValidator>();
        services.AddScoped<ICommandValidator<GetShiftModelByIdQuery>, GetShiftModelByIdValidator>();
        services.AddScoped<ICommandValidator<CreateExternalPersonCommand>, CreateExternalPersonValidator>();
        services.AddScoped<ICommandValidator<CreateUserDelegationCommand>, CreateUserDelegationValidator>();

        services.AddScoped<IExternalPersonReadRepository, ExternalPersonReadRepository>();
        services.AddScoped<IExternalPersonWriteRepository, ExternalPersonWriteRepository>();
        services.AddScoped<IUserDelegationReadRepository, UserDelegationReadRepository>();
        services.AddScoped<IUserDelegationWriteRepository, UserDelegationWriteRepository>();

        services.AddScoped<ICommandValidator<CreateMachineCommand>, CreateMachineValidator>();
        services.AddScoped<ICommandValidator<UpdateMachineCommand>, UpdateMachineValidator>();
        services.AddScoped<ICommandValidator<DeactivateMachineCommand>, DeactivateMachineValidator>();
        services.AddScoped<ICommandValidator<GetVisibleMachineByIdQuery>, GetVisibleMachineByIdValidator>();

        services.AddScoped<ICommandValidator<CreateMachineComponentAreaCommand>, CreateMachineComponentAreaValidator>();
        services.AddScoped<ICommandValidator<UpdateMachineComponentAreaCommand>, UpdateMachineComponentAreaValidator>();
        services.AddScoped<ICommandValidator<SetMachineComponentAreaActiveStateCommand>, SetMachineComponentAreaActiveStateValidator>();
        services.AddScoped<ICommandValidator<GetVisibleMachineComponentAreasQuery>, GetVisibleMachineComponentAreasValidator>();

        services.AddScoped<ICommandValidator<CreateMachineComponentCommand>, CreateMachineComponentValidator>();
        services.AddScoped<ICommandValidator<UpdateMachineComponentCommand>, UpdateMachineComponentValidator>();
        services.AddScoped<ICommandValidator<MoveMachineComponentCommand>, MoveMachineComponentValidator>();
        services.AddScoped<ICommandValidator<SetMachineComponentActiveStateCommand>, SetMachineComponentActiveStateValidator>();
        services.AddScoped<ICommandValidator<GetVisibleMachineComponentsByMachineQuery>, GetVisibleMachineComponentsByMachineValidator>();

        services.AddScoped<ICommandValidator<CreateMaintenanceOrderCommand>, CreateMaintenanceOrderValidator>();
        services.AddScoped<ICommandValidator<UpdateMaintenanceOrderCommand>, UpdateMaintenanceOrderValidator>();
        services.AddScoped<ICommandValidator<CreateMaintenancePlanCommand>, CreateMaintenancePlanValidator>();
        services.AddScoped<ICommandValidator<UpdateMaintenancePlanCommand>, UpdateMaintenancePlanValidator>();

        services.AddScoped<ICommandValidator<CreateAccessRuleCommand>, CreateAccessRuleValidator>();
        services.AddScoped<ICommandValidator<UpdateAccessRuleCommand>, UpdateAccessRuleValidator>();
        services.AddScoped<ICommandValidator<DeactivateAccessRuleCommand>, DeactivateAccessRuleValidator>();
        services.AddScoped<ICommandValidator<GetAccessRulesByTargetQuery>, GetAccessRulesByTargetValidator>();

        services.AddScoped<ICommandValidator<CreateInventoryItemCommand>, CreateInventoryItemValidator>();
        services.AddScoped<ICommandValidator<UpdateInventoryItemCommand>, UpdateInventoryItemValidator>();
        services.AddScoped<ICommandValidator<DeactivateInventoryItemCommand>, DeactivateInventoryItemValidator>();

        services.AddScoped<ICommandValidator<CreateLocationCommand>, CreateLocationValidator>();
        services.AddScoped<ICommandValidator<UpdateLocationCommand>, UpdateLocationValidator>();

        services.AddScoped<ICommandValidator<CreateStockMovementCommand>, CreateStockMovementValidator>();
        services.AddScoped<ICommandValidator<GetStockAvailabilityQuery>, GetStockAvailabilityValidator>();
        services.AddScoped<ICommandValidator<CreateStockReservationCommand>, CreateStockReservationValidator>();
        services.AddScoped<ICommandValidator<ConsumeStockReservationCommand>, ConsumeStockReservationValidator>();
        services.AddScoped<ICommandValidator<ReleaseStockReservationCommand>, ReleaseStockReservationValidator>();

        services.AddScoped<ICommandValidator<CreateEnergyMeterCommand>, CreateEnergyMeterValidator>();
        services.AddScoped<ICommandValidator<UpdateEnergyMeterCommand>, UpdateEnergyMeterValidator>();
        services.AddScoped<ICommandValidator<CreateEnergyReadingCommand>, CreateEnergyReadingValidator>();
        services.AddScoped<ICommandValidator<UpdateEnergyReadingCommand>, UpdateEnergyReadingValidator>();
        services.AddScoped<ICommandValidator<GetEnergyAnalysisQuery>, GetEnergyAnalysisValidator>();
        services.AddScoped<ICommandValidator<CreateBugCommand>, CreateBugValidator>();
        services.AddScoped<ICommandValidator<UpdateBugCommand>, UpdateBugValidator>();
        services.AddScoped<ICommandValidator<CreateChangeRequestCommand>, CreateChangeRequestValidator>();
        services.AddScoped<ICommandValidator<UpdateChangeRequestCommand>, UpdateChangeRequestValidator>();
        services.AddScoped<ICommandValidator<CreateSupportCaseCommand>, CreateSupportCaseValidator>();
        services.AddScoped<ICommandValidator<UpdateSupportCaseCommand>, UpdateSupportCaseValidator>();
        services.AddScoped<ICommandValidator<EscalateSupportCaseToBugCommand>, EscalateSupportCaseToBugValidator>();
        services.AddScoped<ICommandValidator<EscalateSupportCaseToIncidentCommand>, EscalateSupportCaseToIncidentValidator>();
        services.AddScoped<ICommandValidator<CreateIncidentCommand>, CreateIncidentValidator>();
        services.AddScoped<ICommandValidator<UpdateIncidentCommand>, UpdateIncidentValidator>();
        services.AddScoped<ICommandValidator<CreateKnowledgeEntryCommand>, CreateKnowledgeEntryValidator>();
        services.AddScoped<ICommandValidator<UpdateKnowledgeEntryCommand>, UpdateKnowledgeEntryValidator>();
        services.AddScoped<ICommandValidator<CreateServiceLevelCommand>, CreateServiceLevelValidator>();
        services.AddScoped<ICommandValidator<UpdateServiceLevelCommand>, UpdateServiceLevelValidator>();

        services.AddScoped<IAuthorizationPolicy<CreateUserCommand>, CreateUserPolicy>();
        services.AddScoped<IAuthorizationPolicy<UpdateUserCommand>, UpdateUserPolicy>();
        services.AddScoped<IAuthorizationPolicy<ChangeUserStatusCommand>, ChangeUserStatusPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetUsersQuery>, GetUsersPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetUserByIdQuery>, GetUserByIdPolicy>();

        services.AddScoped<IAuthorizationPolicy<CreateRoleCommand>, CreateRolePolicy>();
        services.AddScoped<IAuthorizationPolicy<UpdateRoleCommand>, UpdateRolePolicy>();
        services.AddScoped<IAuthorizationPolicy<GetRolesQuery>, GetRolesPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetRoleByIdQuery>, GetRoleByIdPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetPermissionsQuery>, GetPermissionsPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetRolePermissionsQuery>, GetRolePermissionsPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetSecurityUsersQuery>, GetSecurityUsersPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetSecurityUserByIdQuery>, GetSecurityUserByIdPolicy>();
        services.AddScoped<IAuthorizationPolicy<SetRolePermissionsCommand>, SetRolePermissionsPolicy>();

        services.AddScoped<IAuthorizationPolicy<CreateOrganizationUnitCommand>, CreateOrganizationUnitPolicy>();
        services.AddScoped<IAuthorizationPolicy<UpdateOrganizationUnitCommand>, UpdateOrganizationUnitPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetOrganizationUnitsQuery>, GetOrganizationUnitsPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetOrganizationUnitByIdQuery>, GetOrganizationUnitByIdPolicy>();

        services.AddScoped<IAuthorizationPolicy<CreateCostCenterCommand>, CreateCostCenterPolicy>();
        services.AddScoped<IAuthorizationPolicy<UpdateCostCenterCommand>, UpdateCostCenterPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetCostCentersQuery>, GetCostCentersPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetCostCenterByIdQuery>, GetCostCenterByIdPolicy>();

        services.AddScoped<IAuthorizationPolicy<CreateShiftModelCommand>, CreateShiftModelPolicy>();
        services.AddScoped<IAuthorizationPolicy<UpdateShiftModelCommand>, UpdateShiftModelPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetShiftModelsQuery>, GetShiftModelsPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetShiftModelByIdQuery>, GetShiftModelByIdPolicy>();

        services.AddScoped<IAuthorizationPolicy<AssignRoleToUserCommand>, AssignRoleToUserPolicy>();
        services.AddScoped<IAuthorizationPolicy<RemoveRoleFromUserCommand>, RemoveRoleFromUserPolicy>();
        services.AddScoped<IAuthorizationPolicy<AssignOrganizationUnitToUserCommand>, AssignOrganizationUnitToUserPolicy>();
        services.AddScoped<IAuthorizationPolicy<RemoveOrganizationUnitFromUserCommand>, RemoveOrganizationUnitFromUserPolicy>();

        services.AddScoped<IAuthorizationPolicy<CreateMachineCommand>, CreateMachinePolicy>();
        services.AddScoped<IAuthorizationPolicy<UpdateMachineCommand>, UpdateMachinePolicy>();
        services.AddScoped<IAuthorizationPolicy<DeactivateMachineCommand>, DeactivateMachinePolicy>();
        services.AddScoped<IAuthorizationPolicy<GetVisibleMachinesQuery>, GetVisibleMachinesPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetVisibleMachineByIdQuery>, GetVisibleMachineByIdPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetMachineSelectionOptionsQuery>, GetMachineSelectionOptionsPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetOrganizationUnitSelectionOptionsQuery>, GetOrganizationUnitSelectionOptionsPolicy>();

        services.AddScoped<IAuthorizationPolicy<CreateMachineComponentAreaCommand>, CreateMachineComponentAreaPolicy>();
        services.AddScoped<IAuthorizationPolicy<UpdateMachineComponentAreaCommand>, UpdateMachineComponentAreaPolicy>();
        services.AddScoped<IAuthorizationPolicy<SetMachineComponentAreaActiveStateCommand>, SetMachineComponentAreaActiveStatePolicy>();
        services.AddScoped<IAuthorizationPolicy<GetVisibleMachineComponentAreasQuery>, GetVisibleMachineComponentAreasPolicy>();

        services.AddScoped<IAuthorizationPolicy<CreateMachineComponentCommand>, CreateMachineComponentPolicy>();
        services.AddScoped<IAuthorizationPolicy<UpdateMachineComponentCommand>, UpdateMachineComponentPolicy>();
        services.AddScoped<IAuthorizationPolicy<MoveMachineComponentCommand>, MoveMachineComponentPolicy>();
        services.AddScoped<IAuthorizationPolicy<SetMachineComponentActiveStateCommand>, SetMachineComponentActiveStatePolicy>();
        services.AddScoped<IAuthorizationPolicy<GetVisibleMachineComponentsByMachineQuery>, GetVisibleMachineComponentsByMachinePolicy>();

        services.AddScoped<IAuthorizationPolicy<CreateMaintenanceOrderCommand>, CreateMaintenanceOrderPolicy>();
        services.AddScoped<IAuthorizationPolicy<UpdateMaintenanceOrderCommand>, UpdateMaintenanceOrderPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetMaintenanceOrdersQuery>, GetMaintenanceOrdersPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetMaintenanceOrderByIdQuery>, GetMaintenanceOrderByIdPolicy>();
        services.AddScoped<IAuthorizationPolicy<CreateMaintenancePlanCommand>, CreateMaintenancePlanPolicy>();
        services.AddScoped<IAuthorizationPolicy<UpdateMaintenancePlanCommand>, UpdateMaintenancePlanPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetMaintenancePlansQuery>, GetMaintenancePlansPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetMaintenancePlanByIdQuery>, GetMaintenancePlanByIdPolicy>();

        services.AddScoped<IAuthorizationPolicy<CreateAccessRuleCommand>, CreateAccessRulePolicy>();
        services.AddScoped<IAuthorizationPolicy<UpdateAccessRuleCommand>, UpdateAccessRulePolicy>();
        services.AddScoped<IAuthorizationPolicy<DeactivateAccessRuleCommand>, DeactivateAccessRulePolicy>();
        services.AddScoped<IAuthorizationPolicy<GetAccessRulesByTargetQuery>, GetAccessRulesByTargetPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetAuditEntriesQuery>, GetAuditEntriesPolicy>();

        services.AddScoped<IAuthorizationPolicy<CreateInventoryItemCommand>, CreateInventoryItemPolicy>();
        services.AddScoped<IAuthorizationPolicy<UpdateInventoryItemCommand>, UpdateInventoryItemPolicy>();
        services.AddScoped<IAuthorizationPolicy<DeactivateInventoryItemCommand>, DeactivateInventoryItemPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetInventoryItemsQuery>, GetInventoryItemsPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetInventoryItemByIdQuery>, GetInventoryItemByIdPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetInventoryItemLookupQuery>, GetInventoryItemLookupPolicy>();

        services.AddScoped<IAuthorizationPolicy<CreateLocationCommand>, CreateLocationPolicy>();
        services.AddScoped<IAuthorizationPolicy<UpdateLocationCommand>, UpdateLocationPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetLocationsQuery>, GetLocationsPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetLocationByIdQuery>, GetLocationByIdPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetLocationLookupQuery>, GetLocationLookupPolicy>();

        services.AddScoped<IAuthorizationPolicy<CreateStockReservationCommand>, CreateStockReservationPolicy>();
        services.AddScoped<IAuthorizationPolicy<ConsumeStockReservationCommand>, ConsumeStockReservationPolicy>();
        services.AddScoped<IAuthorizationPolicy<ReleaseStockReservationCommand>, ReleaseStockReservationPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetStockReservationsQuery>, GetStockReservationsPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetStockReservationByIdQuery>, GetStockReservationByIdPolicy>();

        services.AddScoped<IAuthorizationPolicy<CreateStockMovementCommand>, CreateStockMovementPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetStockMovementsQuery>, GetStockMovementsPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetStockMovementByIdQuery>, GetStockMovementByIdPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetStockAvailabilityQuery>, GetStockAvailabilityPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetStockLookupQuery>, GetStockLookupPolicy>();

        services.AddScoped<IAuthorizationPolicy<CreateEnergyMeterCommand>, CreateEnergyMeterPolicy>();
        services.AddScoped<IAuthorizationPolicy<UpdateEnergyMeterCommand>, UpdateEnergyMeterPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetEnergyMetersQuery>, GetEnergyMetersPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetEnergyMeterByIdQuery>, GetEnergyMeterByIdPolicy>();
        services.AddScoped<IAuthorizationPolicy<CreateEnergyReadingCommand>, CreateEnergyReadingPolicy>();
        services.AddScoped<IAuthorizationPolicy<UpdateEnergyReadingCommand>, UpdateEnergyReadingPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetEnergyReadingsQuery>, GetEnergyReadingsPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetEnergyReadingByIdQuery>, GetEnergyReadingByIdPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetEnergyAnalysisQuery>, GetEnergyAnalysisPolicy>();
        services.AddScoped<IAuthorizationPolicy<CreateBugCommand>, CreateBugPolicy>();
        services.AddScoped<IAuthorizationPolicy<UpdateBugCommand>, UpdateBugPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetBugsQuery>, GetBugsPolicy>();
        services.AddScoped<IAuthorizationPolicy<CreateChangeRequestCommand>, CreateChangeRequestPolicy>();
        services.AddScoped<IAuthorizationPolicy<UpdateChangeRequestCommand>, UpdateChangeRequestPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetChangeRequestsQuery>, GetChangeRequestsPolicy>();
        services.AddScoped<IAuthorizationPolicy<CreateSupportCaseCommand>, CreateSupportCasePolicy>();
        services.AddScoped<IAuthorizationPolicy<UpdateSupportCaseCommand>, UpdateSupportCasePolicy>();
        services.AddScoped<IAuthorizationPolicy<EscalateSupportCaseToBugCommand>, EscalateSupportCaseToBugPolicy>();
        services.AddScoped<IAuthorizationPolicy<EscalateSupportCaseToIncidentCommand>, EscalateSupportCaseToIncidentPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetSupportCasesQuery>, GetSupportCasesPolicy>();
        services.AddScoped<IAuthorizationPolicy<CreateIncidentCommand>, CreateIncidentPolicy>();
        services.AddScoped<IAuthorizationPolicy<UpdateIncidentCommand>, UpdateIncidentPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetIncidentsQuery>, GetIncidentsPolicy>();
        services.AddScoped<IAuthorizationPolicy<CreateKnowledgeEntryCommand>, CreateKnowledgeEntryPolicy>();
        services.AddScoped<IAuthorizationPolicy<UpdateKnowledgeEntryCommand>, UpdateKnowledgeEntryPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetKnowledgeEntriesQuery>, GetKnowledgeEntriesPolicy>();
        services.AddScoped<IAuthorizationPolicy<CreateServiceLevelCommand>, CreateServiceLevelPolicy>();
        services.AddScoped<IAuthorizationPolicy<UpdateServiceLevelCommand>, UpdateServiceLevelPolicy>();
        services.AddScoped<IAuthorizationPolicy<GetServiceLevelsQuery>, GetServiceLevelsPolicy>();

        services.AddScoped<IUseCaseHandler<CreateUserCommand, Guid>, CreateUserHandler>();
        services.AddScoped<IUseCaseHandler<GetUsersQuery, IReadOnlyList<UserListItem>>, GetUsersHandler>();
        services.AddScoped<IUseCaseHandler<GetUserByIdQuery, UserDetailsDto>, GetUserByIdHandler>();
        services.AddScoped<IUseCaseHandler<UpdateUserCommand, bool>, UpdateUserHandler>();
        services.AddScoped<IUseCaseHandler<ChangeUserStatusCommand, bool>, ChangeUserStatusHandler>();

        services.AddScoped<IUseCaseHandler<CreateRoleCommand, Guid>, CreateRoleHandler>();
        services.AddScoped<IUseCaseHandler<UpdateRoleCommand, bool>, UpdateRoleHandler>();
        services.AddScoped<IUseCaseHandler<GetRolesQuery, IReadOnlyList<RoleListItem>>, GetRolesHandler>();
        services.AddScoped<IUseCaseHandler<GetRoleByIdQuery, RoleDetailsDto>, GetRoleByIdHandler>();
        services.AddScoped<IUseCaseHandler<GetPermissionsQuery, IReadOnlyList<PermissionListItemDto>>, GetPermissionsHandler>();
        services.AddScoped<IUseCaseHandler<GetRolePermissionsQuery, IReadOnlyList<RolePermissionListItemDto>>, GetRolePermissionsHandler>();
        services.AddScoped<IUseCaseHandler<GetSecurityUsersQuery, IReadOnlyList<SecurityUserListItemDto>>, GetSecurityUsersHandler>();
        services.AddScoped<IUseCaseHandler<GetSecurityUserByIdQuery, SecurityUserDetailsDto>, GetSecurityUserByIdHandler>();
        services.AddScoped<IUseCaseHandler<SetRolePermissionsCommand, bool>, SetRolePermissionsHandler>();

        services.AddScoped<IUseCaseHandler<CreateOrganizationUnitCommand, Guid>, CreateOrganizationUnitHandler>();
        services.AddScoped<IUseCaseHandler<UpdateOrganizationUnitCommand, bool>, UpdateOrganizationUnitHandler>();
        services.AddScoped<IUseCaseHandler<GetOrganizationUnitsQuery, IReadOnlyList<OrganizationUnitListItem>>, GetOrganizationUnitsHandler>();
        services.AddScoped<IUseCaseHandler<GetOrganizationUnitByIdQuery, OrganizationUnitDetailsDto>, GetOrganizationUnitByIdHandler>();

        services.AddScoped<IUseCaseHandler<CreateCostCenterCommand, Guid>, CreateCostCenterHandler>();
        services.AddScoped<IUseCaseHandler<UpdateCostCenterCommand, bool>, UpdateCostCenterHandler>();
        services.AddScoped<IUseCaseHandler<GetCostCentersQuery, IReadOnlyList<CostCenterListItem>>, GetCostCentersHandler>();
        services.AddScoped<IUseCaseHandler<GetCostCenterByIdQuery, CostCenterDetailsDto>, GetCostCenterByIdHandler>();

        services.AddScoped<IUseCaseHandler<CreateShiftModelCommand, Guid>, CreateShiftModelHandler>();
        services.AddScoped<IUseCaseHandler<UpdateShiftModelCommand, bool>, UpdateShiftModelHandler>();
        services.AddScoped<IUseCaseHandler<GetShiftModelsQuery, IReadOnlyList<ShiftModelListItem>>, GetShiftModelsHandler>();
        services.AddScoped<IUseCaseHandler<GetShiftModelByIdQuery, ShiftModelDetailsDto>, GetShiftModelByIdHandler>();

        services.AddScoped<IUseCaseHandler<AssignRoleToUserCommand, bool>, AssignRoleToUserHandler>();
        services.AddScoped<IUseCaseHandler<RemoveRoleFromUserCommand, bool>, RemoveRoleFromUserHandler>();
        services.AddScoped<IUseCaseHandler<AssignOrganizationUnitToUserCommand, bool>, AssignOrganizationUnitToUserHandler>();
        services.AddScoped<IUseCaseHandler<RemoveOrganizationUnitFromUserCommand, bool>, RemoveOrganizationUnitFromUserHandler>();

        services.AddScoped<IUseCaseHandler<CreateMachineCommand, Guid>, CreateMachineHandler>();
        services.AddScoped<IUseCaseHandler<UpdateMachineCommand, bool>, UpdateMachineHandler>();
        services.AddScoped<IUseCaseHandler<DeactivateMachineCommand, bool>, DeactivateMachineHandler>();
        services.AddScoped<IUseCaseHandler<GetVisibleMachinesQuery, IReadOnlyList<MachineListItem>>, GetVisibleMachinesHandler>();
        services.AddScoped<IUseCaseHandler<GetVisibleMachineByIdQuery, MachineDetailsDto>, GetVisibleMachineByIdHandler>();
        services.AddScoped<IUseCaseHandler<GetMachineSelectionOptionsQuery, IReadOnlyList<MachineSelectionOptionDto>>, GetMachineSelectionOptionsHandler>();

        services.AddScoped<
            IUseCaseHandler<
                GetOrganizationUnitSelectionOptionsQuery,
                IReadOnlyList<SWFC.Application.M200_Business.M201_Assets.Machines.OrganizationUnitSelectionOptionDto>>,
            GetOrganizationUnitSelectionOptionsHandler>();

        services.AddScoped<IUseCaseHandler<CreateMachineComponentAreaCommand, Guid>, CreateMachineComponentAreaHandler>();
        services.AddScoped<IUseCaseHandler<UpdateMachineComponentAreaCommand, bool>, UpdateMachineComponentAreaHandler>();
        services.AddScoped<IUseCaseHandler<SetMachineComponentAreaActiveStateCommand, bool>, SetMachineComponentAreaActiveStateHandler>();
        services.AddScoped<IUseCaseHandler<GetVisibleMachineComponentAreasQuery, IReadOnlyList<MachineComponentAreaListItemDto>>, GetVisibleMachineComponentAreasHandler>();

        services.AddScoped<IUseCaseHandler<CreateMachineComponentCommand, Guid>, CreateMachineComponentHandler>();
        services.AddScoped<IUseCaseHandler<UpdateMachineComponentCommand, bool>, UpdateMachineComponentHandler>();
        services.AddScoped<IUseCaseHandler<MoveMachineComponentCommand, bool>, MoveMachineComponentHandler>();
        services.AddScoped<IUseCaseHandler<SetMachineComponentActiveStateCommand, bool>, SetMachineComponentActiveStateHandler>();
        services.AddScoped<IUseCaseHandler<GetVisibleMachineComponentsByMachineQuery, IReadOnlyList<MachineComponentListItemDto>>, GetVisibleMachineComponentsByMachineHandler>();

        services.AddScoped<IUseCaseHandler<CreateMaintenanceOrderCommand, Guid>, CreateMaintenanceOrderHandler>();
        services.AddScoped<IUseCaseHandler<UpdateMaintenanceOrderCommand, bool>, UpdateMaintenanceOrderHandler>();
        services.AddScoped<IUseCaseHandler<GetMaintenanceOrdersQuery, IReadOnlyList<MaintenanceOrderListItemDto>>, GetMaintenanceOrdersHandler>();
        services.AddScoped<IUseCaseHandler<GetMaintenanceOrderByIdQuery, MaintenanceOrderDetailsDto>, GetMaintenanceOrderByIdHandler>();
        services.AddScoped<IUseCaseHandler<CreateMaintenancePlanCommand, Guid>, CreateMaintenancePlanHandler>();
        services.AddScoped<IUseCaseHandler<UpdateMaintenancePlanCommand, bool>, UpdateMaintenancePlanHandler>();
        services.AddScoped<IUseCaseHandler<GetMaintenancePlansQuery, IReadOnlyList<MaintenancePlanListItemDto>>, GetMaintenancePlansHandler>();
        services.AddScoped<IUseCaseHandler<GetMaintenancePlanByIdQuery, MaintenancePlanDetailsDto>, GetMaintenancePlanByIdHandler>();

        services.AddScoped<IUseCaseHandler<CreateAccessRuleCommand, Guid>, CreateAccessRuleHandler>();
        services.AddScoped<IUseCaseHandler<UpdateAccessRuleCommand, bool>, UpdateAccessRuleHandler>();
        services.AddScoped<IUseCaseHandler<DeactivateAccessRuleCommand, bool>, DeactivateAccessRuleHandler>();
        services.AddScoped<IUseCaseHandler<GetAccessRulesByTargetQuery, IReadOnlyList<AccessRuleListItemDto>>, GetAccessRulesByTargetHandler>();
        services.AddScoped<IUseCaseHandler<GetAuditEntriesQuery, IReadOnlyList<AuditEntryListItem>>, GetAuditEntriesHandler>();

        services.AddScoped<IUseCaseHandler<GetInventoryItemsQuery, IReadOnlyList<InventoryItemListItem>>, GetInventoryItemsHandler>();
        services.AddScoped<IUseCaseHandler<GetInventoryItemByIdQuery, InventoryItemDetailsDto>, GetInventoryItemByIdHandler>();
        services.AddScoped<IUseCaseHandler<CreateInventoryItemCommand, Guid>, CreateInventoryItemHandler>();
        services.AddScoped<IUseCaseHandler<UpdateInventoryItemCommand, bool>, UpdateInventoryItemHandler>();
        services.AddScoped<IUseCaseHandler<DeactivateInventoryItemCommand, bool>, DeactivateInventoryItemHandler>();
        services.AddScoped<IUseCaseHandler<GetInventoryItemLookupQuery, IReadOnlyList<InventoryItemLookupItem>>, GetInventoryItemLookupHandler>();

        services.AddScoped<IUseCaseHandler<GetLocationsQuery, IReadOnlyList<LocationListItem>>, GetLocationsHandler>();
        services.AddScoped<IUseCaseHandler<GetLocationByIdQuery, LocationDetailsDto>, GetLocationByIdHandler>();
        services.AddScoped<IUseCaseHandler<CreateLocationCommand, Guid>, CreateLocationHandler>();
        services.AddScoped<IUseCaseHandler<UpdateLocationCommand, bool>, UpdateLocationHandler>();
        services.AddScoped<IUseCaseHandler<GetLocationLookupQuery, IReadOnlyList<LocationLookupItem>>, GetLocationLookupHandler>();

        services.AddScoped<IUseCaseHandler<GetStockReservationsQuery, IReadOnlyList<StockReservationListItem>>, GetStockReservationsHandler>();
        services.AddScoped<IUseCaseHandler<GetStockReservationByIdQuery, StockReservationDetailsDto>, GetStockReservationByIdHandler>();
        services.AddScoped<IUseCaseHandler<CreateStockReservationCommand, Guid>, CreateStockReservationHandler>();
        services.AddScoped<IUseCaseHandler<ConsumeStockReservationCommand, Guid>, ConsumeStockReservationHandler>();
        services.AddScoped<IUseCaseHandler<ReleaseStockReservationCommand, bool>, ReleaseStockReservationHandler>();

        services.AddScoped<IUseCaseHandler<GetStockMovementsQuery, IReadOnlyList<StockMovementListItem>>, GetStockMovementsHandler>();
        services.AddScoped<IUseCaseHandler<GetStockMovementByIdQuery, StockMovementDetailsDto>, GetStockMovementByIdHandler>();
        services.AddScoped<IUseCaseHandler<CreateStockMovementCommand, Guid>, CreateStockMovementHandler>();
        services.AddScoped<IUseCaseHandler<GetStockLookupQuery, IReadOnlyList<StockLookupItem>>, GetStockLookupHandler>();

        services.AddScoped<IUseCaseHandler<CreateEnergyMeterCommand, Guid>, CreateEnergyMeterHandler>();
        services.AddScoped<IUseCaseHandler<GetEnergyMetersQuery, IReadOnlyList<EnergyMeterListItemDto>>, GetEnergyMetersHandler>();
        services.AddScoped<IUseCaseHandler<GetEnergyMeterByIdQuery, EnergyMeterDetailsDto>, GetEnergyMeterByIdHandler>();
        services.AddScoped<IUseCaseHandler<UpdateEnergyMeterCommand, Guid>, UpdateEnergyMeterHandler>();

        services.AddScoped<IUseCaseHandler<CreateEnergyReadingCommand, Guid>, CreateEnergyReadingHandler>();
        services.AddScoped<IUseCaseHandler<GetEnergyReadingsQuery, IReadOnlyList<EnergyReadingListItemDto>>, GetEnergyReadingsHandler>();
        services.AddScoped<IUseCaseHandler<GetEnergyReadingByIdQuery, EnergyReadingDetailsDto>, GetEnergyReadingByIdHandler>();
        services.AddScoped<IUseCaseHandler<UpdateEnergyReadingCommand, Guid>, UpdateEnergyReadingHandler>();

        services.AddScoped<IUseCaseHandler<GetEnergyAnalysisQuery, EnergyAnalysisResultDto>, GetEnergyAnalysisHandler>();
        services.AddScoped<IUseCaseHandler<CreateBugCommand, Guid>, CreateBugHandler>();
        services.AddScoped<IUseCaseHandler<UpdateBugCommand, bool>, UpdateBugHandler>();
        services.AddScoped<IUseCaseHandler<GetBugsQuery, IReadOnlyList<BugListItem>>, GetBugsHandler>();
        services.AddScoped<IUseCaseHandler<CreateChangeRequestCommand, Guid>, CreateChangeRequestHandler>();
        services.AddScoped<IUseCaseHandler<UpdateChangeRequestCommand, bool>, UpdateChangeRequestHandler>();
        services.AddScoped<IUseCaseHandler<GetChangeRequestsQuery, IReadOnlyList<ChangeRequestListItem>>, GetChangeRequestsHandler>();
        services.AddScoped<IUseCaseHandler<CreateSupportCaseCommand, Guid>, CreateSupportCaseHandler>();
        services.AddScoped<IUseCaseHandler<UpdateSupportCaseCommand, bool>, UpdateSupportCaseHandler>();
        services.AddScoped<IUseCaseHandler<EscalateSupportCaseToBugCommand, Guid>, EscalateSupportCaseToBugHandler>();
        services.AddScoped<IUseCaseHandler<EscalateSupportCaseToIncidentCommand, Guid>, EscalateSupportCaseToIncidentHandler>();
        services.AddScoped<IUseCaseHandler<GetSupportCasesQuery, IReadOnlyList<SupportCaseListItem>>, GetSupportCasesHandler>();
        services.AddScoped<IUseCaseHandler<CreateIncidentCommand, Guid>, CreateIncidentHandler>();
        services.AddScoped<IUseCaseHandler<UpdateIncidentCommand, bool>, UpdateIncidentHandler>();
        services.AddScoped<IUseCaseHandler<GetIncidentsQuery, IReadOnlyList<IncidentListItem>>, GetIncidentsHandler>();
        services.AddScoped<IUseCaseHandler<CreateKnowledgeEntryCommand, Guid>, CreateKnowledgeEntryHandler>();
        services.AddScoped<IUseCaseHandler<UpdateKnowledgeEntryCommand, bool>, UpdateKnowledgeEntryHandler>();
        services.AddScoped<IUseCaseHandler<GetKnowledgeEntriesQuery, IReadOnlyList<KnowledgeEntryListItem>>, GetKnowledgeEntriesHandler>();
        services.AddScoped<IUseCaseHandler<CreateServiceLevelCommand, Guid>, CreateServiceLevelHandler>();
        services.AddScoped<IUseCaseHandler<UpdateServiceLevelCommand, bool>, UpdateServiceLevelHandler>();
        services.AddScoped<IUseCaseHandler<GetServiceLevelsQuery, IReadOnlyList<ServiceLevelListItem>>, GetServiceLevelsHandler>();

        services.AddScoped<CreatePurchaseRequirement>();
        services.AddScoped<GetPurchaseRequirements>();
        services.AddScoped<GetSuppliers>();
        services.AddScoped<CreateSupplier>();
        services.AddScoped<GetPurchaseOrders>();
        services.AddScoped<CreatePurchaseOrder>();
        services.AddScoped<GetGoodsReceipts>();
        services.AddScoped<CreateGoodsReceipt>();
        services.AddScoped<GetRequestForQuotations>();
        services.AddScoped<CreateRequestForQuotation>();

        return services;
    }
}





