using Microsoft.EntityFrameworkCore;
using SWFC.Domain.M100_System.M102_Organization.Assignments;
using SWFC.Domain.M100_System.M102_Organization.OrganizationUnits;
using SWFC.Domain.M100_System.M102_Organization.Users;
using SWFC.Domain.M200_Business.M201_Assets.MachineComponentAreas;
using SWFC.Domain.M200_Business.M201_Assets.MachineComponents;
using SWFC.Domain.M200_Business.M201_Assets.Machines;
using SWFC.Domain.M200_Business.M202_Maintenance.MaintenanceOrders;
using SWFC.Domain.M200_Business.M202_Maintenance.MaintenancePlans;
using SWFC.Domain.M200_Business.M204_Inventory.Items;
using SWFC.Domain.M200_Business.M204_Inventory.Locations;
using SWFC.Domain.M200_Business.M204_Inventory.Reservations;
using SWFC.Domain.M200_Business.M204_Inventory.Stock;
using SWFC.Domain.M200_Business.M205_Energy.EnergyMeters;
using SWFC.Domain.M200_Business.M205_Energy.EnergyReadings;
using SWFC.Domain.M800_Security.M805_AuditCompliance.Entities;
using SWFC.Domain.M800_Security.M801_Access;
using SWFC.Domain.M800_Security.M806_AccessControl.Assignments;
using SWFC.Domain.M800_Security.M806_AccessControl.Permissions;
using SWFC.Domain.M800_Security.M806_AccessControl.RolePermissions;
using SWFC.Domain.M800_Security.M806_AccessControl.Roles;
using SWFC.Infrastructure.M100_System.M103_Authentication.Entities;
using SWFC.Infrastructure.M100_System.M107_SetupDeployment.Entities;
using SWFC.Infrastructure.Persistence.Configurations.M100_System;
using SWFC.Infrastructure.Persistence.Configurations.M103_Authentication;
using SWFC.Infrastructure.Persistence.Configurations.M200_Business;
using SWFC.Infrastructure.Persistence.Configurations.M200_Business.M205_Energy;
using SWFC.Infrastructure.Persistence.Configurations.M700_Support;
using SWFC.Infrastructure.Persistence.Configurations.M800_Security;
using SWFC.Domain.M100_System.M102_Organization.CostCenters;
using SWFC.Domain.M100_System.M102_Organization.ShiftModels;
using SWFC.Domain.M700_Support.M701_BugTracking;
using SWFC.Domain.M700_Support.M702_ChangeRequests;
using SWFC.Domain.M700_Support.M703_SupportCases;
using SWFC.Domain.M700_Support.M704_Incident_Management;
using SWFC.Domain.M700_Support.M705_Knowledge_Base;
using SWFC.Domain.M700_Support.M706_SLA_Service_Levels;
using SWFC.Domain.M200_Business.M206_Purchasing.GoodsReceipts;
using SWFC.Domain.M200_Business.M206_Purchasing.PurchaseOrders;
using SWFC.Domain.M200_Business.M206_Purchasing.PurchaseRequirements;
using SWFC.Domain.M200_Business.M206_Purchasing.RequestForQuotations;
using SWFC.Domain.M200_Business.M206_Purchasing.Suppliers;
using SWFC.Infrastructure.Persistence.Configurations.M200_Business.M206_Purchasing;

namespace SWFC.Infrastructure.Persistence.Context;

public sealed class AppDbContext : DbContext
{
    public const string DefaultSchema = "core";

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<OrganizationUnit> OrganizationUnits => Set<OrganizationUnit>();
    public DbSet<UserHistoryEntry> UserHistoryEntries => Set<UserHistoryEntry>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserOrganizationUnit> UserOrganizationUnits => Set<UserOrganizationUnit>();
    public DbSet<LocalCredential> LocalCredentials => Set<LocalCredential>();
    public DbSet<SetupState> SetupStates => Set<SetupState>();
    public DbSet<Machine> Machines => Set<Machine>();
    public DbSet<MachineComponentArea> MachineComponentAreas => Set<MachineComponentArea>();
    public DbSet<MachineComponent> MachineComponents => Set<MachineComponent>();
    public DbSet<MaintenanceOrder> MaintenanceOrders => Set<MaintenanceOrder>();
    public DbSet<MaintenancePlan> MaintenancePlans => Set<MaintenancePlan>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<Stock> Stocks => Set<Stock>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<StockReservation> StockReservations => Set<StockReservation>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<EnergyMeter> EnergyMeters => Set<EnergyMeter>();
    public DbSet<EnergyReading> EnergyReadings => Set<EnergyReading>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<AccessRule> AccessRules => Set<AccessRule>();
    public DbSet<CostCenter> CostCenters => Set<CostCenter>();
    public DbSet<ShiftModel> ShiftModels => Set<ShiftModel>();
    public DbSet<Bug> Bugs => Set<Bug>();
    public DbSet<ChangeRequest> ChangeRequests => Set<ChangeRequest>();
    public DbSet<SupportCase> SupportCases => Set<SupportCase>();
    public DbSet<Incident> Incidents => Set<Incident>();
    public DbSet<KnowledgeEntry> KnowledgeEntries => Set<KnowledgeEntry>();
    public DbSet<ServiceLevel> ServiceLevels => Set<ServiceLevel>();
    public DbSet<PurchaseRequirement> PurchaseRequirements => Set<PurchaseRequirement>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<GoodsReceipt> GoodsReceipts => Set<GoodsReceipt>();
    public DbSet<RequestForQuotation> RequestForQuotations => Set<RequestForQuotation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(DefaultSchema);

        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new PermissionConfiguration());
        modelBuilder.ApplyConfiguration(new OrganizationUnitConfiguration());
        modelBuilder.ApplyConfiguration(new UserHistoryEntryConfiguration());
        modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
        modelBuilder.ApplyConfiguration(new RolePermissionConfiguration());
        modelBuilder.ApplyConfiguration(new UserOrganizationUnitConfiguration());
        modelBuilder.ApplyConfiguration(new LocalCredentialConfiguration());
        modelBuilder.ApplyConfiguration(new SetupStateConfiguration());
        modelBuilder.ApplyConfiguration(new MachineConfiguration());
        modelBuilder.ApplyConfiguration(new MachineComponentAreaConfiguration());
        modelBuilder.ApplyConfiguration(new MachineComponentConfiguration());
        modelBuilder.ApplyConfiguration(new MaintenanceOrderConfiguration());
        modelBuilder.ApplyConfiguration(new MaintenancePlanConfiguration());
        modelBuilder.ApplyConfiguration(new EnergyMeterConfiguration());
        modelBuilder.ApplyConfiguration(new EnergyReadingConfiguration());
        modelBuilder.ApplyConfiguration(new PurchaseRequirementConfiguration());
        modelBuilder.ApplyConfiguration(new SupplierConfiguration());
        modelBuilder.ApplyConfiguration(new PurchaseOrderConfiguration());
        modelBuilder.ApplyConfiguration(new GoodsReceiptConfiguration());
        modelBuilder.ApplyConfiguration(new RequestForQuotationConfiguration());
        modelBuilder.ApplyConfiguration(new AccessRuleConfiguration());
        modelBuilder.ApplyConfiguration(new CostCenterConfiguration());
        modelBuilder.ApplyConfiguration(new ShiftModelConfiguration());
        modelBuilder.ApplyConfiguration(new BugConfiguration());
        modelBuilder.ApplyConfiguration(new ChangeRequestConfiguration());
        modelBuilder.ApplyConfiguration(new SupportCaseConfiguration());
        modelBuilder.ApplyConfiguration(new IncidentConfiguration());
        modelBuilder.ApplyConfiguration(new KnowledgeEntryConfiguration());
        modelBuilder.ApplyConfiguration(new ServiceLevelConfiguration());

        modelBuilder.Entity<InventoryItem>(entity =>
        {
            entity.ToTable("InventoryItems");

            entity.HasKey(x => x.Id);

            entity.Ignore(x => x.Stock);

            entity.Property(x => x.IsActive)
                .IsRequired();

            entity.OwnsOne(x => x.ArticleNumber, owned =>
            {
                owned.Property(p => p.Value)
                    .HasColumnName("ArticleNumber")
                    .HasMaxLength(50)
                    .IsRequired();
            });

            entity.OwnsOne(x => x.Name, owned =>
            {
                owned.Property(p => p.Value)
                    .HasColumnName("Name")
                    .HasMaxLength(100)
                    .IsRequired();
            });

            entity.OwnsOne(x => x.Description, owned =>
            {
                owned.Property(p => p.Value)
                    .HasColumnName("Description")
                    .HasMaxLength(500)
                    .IsRequired();
            });

            entity.OwnsOne(x => x.Unit, owned =>
            {
                owned.Property(p => p.Value)
                    .HasColumnName("Unit")
                    .HasMaxLength(20)
                    .IsRequired();
            });

            entity.OwnsOne(x => x.Barcode, owned =>
            {
                owned.Property(p => p.Value)
                    .HasColumnName("Barcode")
                    .HasMaxLength(50)
                    .IsRequired(false);
            });

            entity.OwnsOne(x => x.Manufacturer, owned =>
            {
                owned.Property(p => p.Value)
                    .HasColumnName("Manufacturer")
                    .HasMaxLength(100)
                    .IsRequired(false);
            });

            entity.OwnsOne(x => x.ManufacturerPartNumber, owned =>
            {
                owned.Property(p => p.Value)
                    .HasColumnName("ManufacturerPartNumber")
                    .HasMaxLength(100)
                    .IsRequired(false);
            });

            entity.OwnsOne(x => x.AuditInfo, audit =>
            {
                audit.Property(a => a.CreatedAtUtc)
                    .HasColumnName("CreatedAtUtc")
                    .IsRequired();

                audit.Property(a => a.CreatedBy)
                    .HasColumnName("CreatedBy")
                    .HasMaxLength(200)
                    .IsRequired();

                audit.Property(a => a.LastModifiedAtUtc)
                    .HasColumnName("LastModifiedAtUtc")
                    .IsRequired(false);

                audit.Property(a => a.LastModifiedBy)
                    .HasColumnName("LastModifiedBy")
                    .HasMaxLength(200)
                    .IsRequired(false);
            });

            entity.HasMany(x => x.Stocks)
                .WithOne()
                .HasForeignKey(x => x.InventoryItemId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.ToTable("Locations");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.ParentLocationId)
                .IsRequired(false);

            entity.OwnsOne(x => x.Name, owned =>
            {
                owned.Property(p => p.Value)
                    .HasColumnName("Name")
                    .HasMaxLength(100)
                    .IsRequired();
            });

            entity.OwnsOne(x => x.Code, owned =>
            {
                owned.Property(p => p.Value)
                    .HasColumnName("Code")
                    .HasMaxLength(50)
                    .IsRequired();
            });

            entity.OwnsOne(x => x.AuditInfo, audit =>
            {
                audit.Property(a => a.CreatedAtUtc)
                    .HasColumnName("CreatedAtUtc")
                    .IsRequired();

                audit.Property(a => a.CreatedBy)
                    .HasColumnName("CreatedBy")
                    .HasMaxLength(200)
                    .IsRequired();

                audit.Property(a => a.LastModifiedAtUtc)
                    .HasColumnName("LastModifiedAtUtc")
                    .IsRequired(false);

                audit.Property(a => a.LastModifiedBy)
                    .HasColumnName("LastModifiedBy")
                    .HasMaxLength(200)
                    .IsRequired(false);
            });

            entity.HasOne<Location>()
                .WithMany()
                .HasForeignKey(x => x.ParentLocationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Stock>(entity =>
        {
            entity.ToTable("Stocks");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.InventoryItemId)
                .IsRequired();

            entity.Property(x => x.LocationId)
                .IsRequired();

            entity.Property(x => x.Bin)
                .HasMaxLength(100)
                .IsRequired(false);

            entity.Property(x => x.QuantityOnHand)
                .IsRequired();

            entity.OwnsOne(x => x.AuditInfo, audit =>
            {
                audit.Property(a => a.CreatedAtUtc)
                    .HasColumnName("CreatedAtUtc")
                    .IsRequired();

                audit.Property(a => a.CreatedBy)
                    .HasColumnName("CreatedBy")
                    .HasMaxLength(200)
                    .IsRequired();

                audit.Property(a => a.LastModifiedAtUtc)
                    .HasColumnName("LastModifiedAtUtc")
                    .IsRequired(false);

                audit.Property(a => a.LastModifiedBy)
                    .HasColumnName("LastModifiedBy")
                    .HasMaxLength(200)
                    .IsRequired(false);
            });

            entity.HasOne<InventoryItem>()
                .WithMany(x => x.Stocks)
                .HasForeignKey(x => x.InventoryItemId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Location>()
                .WithMany()
                .HasForeignKey(x => x.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(x => x.Movements)
                .WithOne()
                .HasForeignKey(x => x.StockId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(x => x.Reservations)
                .WithOne()
                .HasForeignKey(x => x.StockId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.InventoryItemId, x.LocationId, x.Bin })
                .IsUnique();
        });

        modelBuilder.Entity<StockMovement>(entity =>
        {
            entity.ToTable("StockMovements");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.StockId)
                .IsRequired();

            entity.Property(x => x.MovementType)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(x => x.QuantityDelta)
                .IsRequired();

            entity.Property(x => x.TargetType)
                .HasConversion<int>()
                .IsRequired(false);

            entity.Property(x => x.TargetReference)
                .HasMaxLength(200)
                .IsRequired(false);

            entity.OwnsOne(x => x.AuditInfo, audit =>
            {
                audit.Property(a => a.CreatedAtUtc)
                    .HasColumnName("CreatedAtUtc")
                    .IsRequired();

                audit.Property(a => a.CreatedBy)
                    .HasColumnName("CreatedBy")
                    .HasMaxLength(200)
                    .IsRequired();

                audit.Property(a => a.LastModifiedAtUtc)
                    .HasColumnName("LastModifiedAtUtc")
                    .IsRequired(false);

                audit.Property(a => a.LastModifiedBy)
                    .HasColumnName("LastModifiedBy")
                    .HasMaxLength(200)
                    .IsRequired(false);
            });

            entity.HasOne<Stock>()
                .WithMany(x => x.Movements)
                .HasForeignKey(x => x.StockId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StockReservation>(entity =>
        {
            entity.ToTable("StockReservations");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.StockId)
                .IsRequired();

            entity.Property(x => x.Quantity)
                .IsRequired();

            entity.Property(x => x.Note)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(x => x.TargetType)
                .HasConversion<int>()
                .IsRequired(false);

            entity.Property(x => x.TargetReference)
                .HasMaxLength(200)
                .IsRequired(false);

            entity.OwnsOne(x => x.AuditInfo, audit =>
            {
                audit.Property(a => a.CreatedAtUtc)
                    .HasColumnName("CreatedAtUtc")
                    .IsRequired();

                audit.Property(a => a.CreatedBy)
                    .HasColumnName("CreatedBy")
                    .HasMaxLength(200)
                    .IsRequired();

                audit.Property(a => a.LastModifiedAtUtc)
                    .HasColumnName("LastModifiedAtUtc")
                    .IsRequired(false);

                audit.Property(a => a.LastModifiedBy)
                    .HasColumnName("LastModifiedBy")
                    .HasMaxLength(200)
                    .IsRequired(false);
            });

            entity.HasOne<Stock>()
                .WithMany(x => x.Reservations)
                .HasForeignKey(x => x.StockId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.ActorUserId).HasColumnName("UserId").IsRequired().HasMaxLength(200);
            entity.Property(x => x.ActorDisplayName).HasColumnName("Username").IsRequired().HasMaxLength(200);
            entity.Property(x => x.Action).IsRequired().HasMaxLength(100);
            entity.Property(x => x.Module).IsRequired().HasMaxLength(50);
            entity.Property(x => x.ObjectType).HasColumnName("Entity").IsRequired().HasMaxLength(100);
            entity.Property(x => x.ObjectId).HasColumnName("EntityId").IsRequired().HasMaxLength(200);
            entity.Property(x => x.TimestampUtc).IsRequired();
            entity.Property(x => x.TargetUserId).HasMaxLength(200).IsRequired(false);
            entity.Property(x => x.OldValues).HasColumnType("text").IsRequired(false);
            entity.Property(x => x.NewValues).HasColumnType("text").IsRequired(false);
            entity.Property(x => x.ClientIp).HasMaxLength(100).IsRequired(false);
            entity.Property(x => x.ClientInfo).HasMaxLength(1000).IsRequired(false);
            entity.Property(x => x.ApprovedByUserId).HasMaxLength(200).IsRequired(false);
            entity.Property(x => x.Reason).HasMaxLength(500).IsRequired(false);

            entity.HasIndex(x => x.TimestampUtc);
            entity.HasIndex(x => x.Action);
            entity.HasIndex(x => x.ActorUserId);
            entity.HasIndex(x => x.TargetUserId);
        });

        base.OnModelCreating(modelBuilder);
    }
}
