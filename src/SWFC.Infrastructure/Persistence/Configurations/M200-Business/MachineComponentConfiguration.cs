using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SWFC.Domain.M200_Business.M201_Assets.MachineComponents;

namespace SWFC.Infrastructure.Persistence.Configurations.M200_Business;

public sealed class MachineComponentConfiguration : IEntityTypeConfiguration<MachineComponent>
{
    public void Configure(EntityTypeBuilder<MachineComponent> entity)
    {
        entity.ToTable("MachineComponents");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.MachineId)
            .IsRequired();

        entity.Property(x => x.MachineComponentAreaId)
            .IsRequired(false);

        entity.Property(x => x.ParentMachineComponentId)
            .IsRequired(false);

        entity.Property(x => x.Name)
            .HasConversion(
                x => x.Value,
                v => MachineComponentName.Create(v))
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(x => x.Description)
            .HasConversion(
                x => x.Value,
                v => MachineComponentDescription.Create(v))
            .IsRequired()
            .HasMaxLength(500);

        entity.Property(x => x.IsActive)
            .IsRequired();

        entity.HasIndex(x => new { x.MachineId, x.Name });

        entity.HasOne<SWFC.Domain.M200_Business.M201_Assets.Machines.Machine>()
            .WithMany()
            .HasForeignKey(x => x.MachineId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne<SWFC.Domain.M200_Business.M201_Assets.MachineComponentAreas.MachineComponentArea>()
            .WithMany()
            .HasForeignKey(x => x.MachineComponentAreaId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne<MachineComponent>()
            .WithMany()
            .HasForeignKey(x => x.ParentMachineComponentId)
            .OnDelete(DeleteBehavior.Restrict);

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
