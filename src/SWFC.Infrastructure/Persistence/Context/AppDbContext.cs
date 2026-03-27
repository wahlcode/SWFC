using Microsoft.EntityFrameworkCore;
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
                audit.Property(a => a.CreatedAtUtc).IsRequired();
                audit.Property(a => a.CreatedBy).IsRequired();
            });
        });

        base.OnModelCreating(modelBuilder);
    }
}