using Microsoft.EntityFrameworkCore;
using SWFC.Domain.M800_Security.M805_AuditCompliance.Entities;
using SWFC.Domain.M200_Business.M201_Assets.Entities;
using SWFC.Domain.M200_Business.M201_Assets.ValueObjects;

namespace SWFC.Infrastructure.Persistence.Context;

public sealed class AppDbContext : DbContext
{
    public const string DefaultSchema = "core";

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Machine> Machines => Set<Machine>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(DefaultSchema);

        modelBuilder.Entity<Machine>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasConversion(
                    x => x.Value,
                    v => MachineName.Create(v))
                .IsRequired()
                .HasMaxLength(100);

            entity.OwnsOne(x => x.AuditInfo, audit =>
            {
                audit.Property(a => a.CreatedAtUtc)
                    .IsRequired();

                audit.Property(a => a.CreatedBy)
                    .IsRequired();

                audit.Property(a => a.LastModifiedAtUtc)
                    .IsRequired(false);

                audit.Property(a => a.LastModifiedBy)
                    .HasMaxLength(200)
                    .IsRequired(false);
            });
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.UserId)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.Username)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.Action)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.Entity)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.EntityId)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.TimestampUtc)
                .IsRequired();

            entity.Property(x => x.OldValues)
                .HasColumnType("text")
                .IsRequired(false);

            entity.Property(x => x.NewValues)
                .HasColumnType("text")
                .IsRequired(false);
        });

        base.OnModelCreating(modelBuilder);
    }
}