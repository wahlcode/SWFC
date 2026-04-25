using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SWFC.Domain.M200_Business.M202_Maintenance.MaintenancePlans;

namespace SWFC.Infrastructure.Persistence.Configurations.M200_Business;

public sealed class MaintenancePlanConfiguration : IEntityTypeConfiguration<MaintenancePlan>
{
    public void Configure(EntityTypeBuilder<MaintenancePlan> builder)
    {
        builder.ToTable("MaintenancePlans");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TargetType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.TargetId)
            .IsRequired();

        builder.Property(x => x.IntervalValue)
            .IsRequired();

        builder.Property(x => x.IntervalUnit)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.NextDueAtUtc)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.OwnsOne(x => x.Name, owned =>
        {
            owned.Property(p => p.Value)
                .HasColumnName("Name")
                .HasMaxLength(200)
                .IsRequired();
        });

        builder.OwnsOne(x => x.Description, owned =>
        {
            owned.Property(p => p.Value)
                .HasColumnName("Description")
                .HasMaxLength(2000)
                .IsRequired();
        });

        builder.OwnsOne(x => x.AuditInfo, audit =>
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
    }
}
