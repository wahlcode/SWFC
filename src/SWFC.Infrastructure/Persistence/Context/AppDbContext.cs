using Microsoft.EntityFrameworkCore;
using SWFC.Domain.M100_System.M102_Organization.Entities;
using SWFC.Domain.M800_Security.M805_AuditCompliance.Entities;
using SWFC.Domain.M200_Business.M201_Assets.Entities;
using SWFC.Domain.M200_Business.M201_Assets.ValueObjects;
using SWFC.Domain.M200_Business.M204_Inventory.Entities;
using SWFC.Domain.M200_Business.M204_Inventory.ValueObjects;
using SWFC.Infrastructure.M800_Security.Auth.Entities;
using SWFC.Infrastructure.Persistence.Configurations.M100_System;
using SWFC.Infrastructure.Persistence.Configurations.M103_Authentication;

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
    public DbSet<OrganizationUnit> OrganizationUnits => Set<OrganizationUnit>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<UserOrganizationUnit> UserOrganizationUnits => Set<UserOrganizationUnit>();

    public DbSet<LocalCredential> LocalCredentials => Set<LocalCredential>();

    public DbSet<Machine> Machines => Set<Machine>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<Stock> Stocks => Set<Stock>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<StockReservation> StockReservations => Set<StockReservation>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(DefaultSchema);

        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new OrganizationUnitConfiguration());
        modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
        modelBuilder.ApplyConfiguration(new UserOrganizationUnitConfiguration());

        modelBuilder.ApplyConfiguration(new LocalCredentialConfiguration());

        modelBuilder.Entity<Machine>(entity =>
        {
            entity.ToTable("Machines");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasConversion(
                    x => x.Value,
                    v => MachineName.Create(v))
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.InventoryNumber)
                .HasConversion(
                    x => x.Value,
                    v => MachineInventoryNumber.Create(v))
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(x => x.Location)
                .HasConversion(
                    x => x.Value,
                    v => MachineLocation.Create(v))
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.Status)
                .HasConversion(
                    x => x.Value,
                    v => MachineStatus.Create(v))
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(x => x.Manufacturer)
                .HasConversion(
                    x => x.Value,
                    v => MachineManufacturer.Create(v))
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.Model)
                .HasConversion(
                    x => x.Value,
                    v => MachineModel.Create(v))
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.SerialNumber)
                .HasConversion(
                    x => x.Value,
                    v => MachineSerialNumber.Create(v))
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.Description)
                .HasConversion(
                    x => x.Value,
                    v => MachineDescription.Create(v))
                .IsRequired()
                .HasMaxLength(500);

            entity.HasIndex(x => x.InventoryNumber)
                .IsUnique();

            entity.OwnsOne(x => x.AuditInfo, audit =>
            {
                audit.Property(a => a.CreatedAtUtc).IsRequired();
                audit.Property(a => a.CreatedBy).IsRequired();
                audit.Property(a => a.LastModifiedAtUtc).IsRequired(false);
                audit.Property(a => a.LastModifiedBy).HasMaxLength(200).IsRequired(false);
            });
        });

        modelBuilder.Entity<InventoryItem>(entity =>
        {
            entity.ToTable("InventoryItems");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasConversion(
                    x => x.Value,
                    v => InventoryItemName.Create(v))
                .IsRequired()
                .HasMaxLength(100);

            entity.OwnsOne(x => x.AuditInfo, audit =>
            {
                audit.Property(a => a.CreatedAtUtc).IsRequired();
                audit.Property(a => a.CreatedBy).IsRequired();
                audit.Property(a => a.LastModifiedAtUtc).IsRequired(false);
                audit.Property(a => a.LastModifiedBy).HasMaxLength(200).IsRequired(false);
            });

            entity.HasOne(x => x.Stock)
                .WithOne()
                .HasForeignKey<Stock>(x => x.InventoryItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Stock>(entity =>
        {
            entity.ToTable("Stocks");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.InventoryItemId)
                .IsRequired();

            entity.Property(x => x.QuantityOnHand)
                .IsRequired();

            entity.HasIndex(x => x.InventoryItemId)
                .IsUnique();

            entity.OwnsOne(x => x.AuditInfo, audit =>
            {
                audit.Property(a => a.CreatedAtUtc).IsRequired();
                audit.Property(a => a.CreatedBy).IsRequired();
                audit.Property(a => a.LastModifiedAtUtc).IsRequired(false);
                audit.Property(a => a.LastModifiedBy).HasMaxLength(200).IsRequired(false);
            });

            entity.HasMany(x => x.Movements)
                .WithOne()
                .HasForeignKey(x => x.StockId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Reservations)
                .WithOne()
                .HasForeignKey(x => x.StockId)
                .OnDelete(DeleteBehavior.Cascade);
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
                audit.Property(a => a.CreatedAtUtc).IsRequired();
                audit.Property(a => a.CreatedBy).IsRequired();
                audit.Property(a => a.LastModifiedAtUtc).IsRequired(false);
                audit.Property(a => a.LastModifiedBy).HasMaxLength(200).IsRequired(false);
            });
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
                audit.Property(a => a.CreatedAtUtc).IsRequired();
                audit.Property(a => a.CreatedBy).IsRequired();
                audit.Property(a => a.LastModifiedAtUtc).IsRequired(false);
                audit.Property(a => a.LastModifiedBy).HasMaxLength(200).IsRequired(false);
            });
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.UserId).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Username).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Action).IsRequired().HasMaxLength(100);
            entity.Property(x => x.Entity).IsRequired().HasMaxLength(100);
            entity.Property(x => x.EntityId).IsRequired().HasMaxLength(200);
            entity.Property(x => x.TimestampUtc).IsRequired();
            entity.Property(x => x.OldValues).HasColumnType("text").IsRequired(false);
            entity.Property(x => x.NewValues).HasColumnType("text").IsRequired(false);
        });

        base.OnModelCreating(modelBuilder);
    }
}