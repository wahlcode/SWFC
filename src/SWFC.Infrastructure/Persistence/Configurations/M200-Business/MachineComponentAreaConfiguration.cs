using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SWFC.Domain.M200_Business.M201_Assets.MachineComponentAreas;

namespace SWFC.Infrastructure.Persistence.Configurations.M200_Business;

public sealed class MachineComponentAreaConfiguration : IEntityTypeConfiguration<MachineComponentArea>
{
    public void Configure(EntityTypeBuilder<MachineComponentArea> entity)
    {
        entity.ToTable("MachineComponentAreas");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Name)
            .HasConversion(
                x => x.Value,
                v => MachineComponentAreaName.Create(v))
            .IsRequired()
            .HasMaxLength(100);

        entity.HasIndex(x => x.Name)
            .IsUnique();

        entity.Property(x => x.IsActive)
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

        entity.Navigation(x => x.AuditInfo)
            .IsRequired();
    }
}
