using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SWFC.Domain.M100_System.M102_Organization.ShiftModels;

namespace SWFC.Infrastructure.Persistence.Configurations.M100_System;

public sealed class ShiftModelConfiguration : IEntityTypeConfiguration<ShiftModel>
{
    public void Configure(EntityTypeBuilder<ShiftModel> entity)
    {
        entity.ToTable("ShiftModels");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Name)
            .HasConversion(
                x => x.Value,
                v => ShiftModelName.Create(v))
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(x => x.Code)
            .HasConversion(
                x => x.Value,
                v => ShiftModelCode.Create(v))
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(x => x.Description)
            .HasMaxLength(1000)
            .IsRequired(false);

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
