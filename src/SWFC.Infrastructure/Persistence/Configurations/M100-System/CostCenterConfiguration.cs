using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SWFC.Domain.M100_System.M102_Organization.CostCenters;

namespace SWFC.Infrastructure.Persistence.Configurations.M100_System;

public sealed class CostCenterConfiguration : IEntityTypeConfiguration<CostCenter>
{
    public void Configure(EntityTypeBuilder<CostCenter> entity)
    {
        entity.ToTable("CostCenters");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Name)
            .HasConversion(
                x => x.Value,
                v => CostCenterName.Create(v))
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(x => x.Code)
            .HasConversion(
                x => x.Value,
                v => CostCenterCode.Create(v))
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(x => x.ParentCostCenterId)
            .IsRequired(false);

        entity.Property(x => x.ValidFrom)
            .IsRequired();

        entity.Property(x => x.IsActive)
            .IsRequired();

        entity.HasIndex(x => x.Code)
            .IsUnique();

        entity.OwnsOne(x => x.AuditInfo, audit =>
        {
            audit.Property(a => a.CreatedAtUtc).IsRequired();
            audit.Property(a => a.CreatedBy).IsRequired();
            audit.Property(a => a.LastModifiedAtUtc).IsRequired(false);
            audit.Property(a => a.LastModifiedBy).HasMaxLength(200).IsRequired(false);
        });
    }
}