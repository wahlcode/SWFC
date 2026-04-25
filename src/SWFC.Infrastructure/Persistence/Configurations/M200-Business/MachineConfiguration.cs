using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SWFC.Domain.M200_Business.M201_Assets.Machines;

namespace SWFC.Infrastructure.Persistence.Configurations.M200_Business;

public sealed class MachineConfiguration : IEntityTypeConfiguration<Machine>
{
    public void Configure(EntityTypeBuilder<Machine> entity)
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

        entity.HasIndex(x => x.InventoryNumber)
            .IsUnique();

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

        entity.Property(x => x.ParentMachineId)
            .IsRequired(false);

        entity.Property(x => x.OrganizationUnitId)
            .IsRequired(false);

        entity.Property(x => x.IsActive)
            .IsRequired();

        entity.HasOne(x => x.ParentMachine)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentMachineId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.Navigation(x => x.Children)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        entity.OwnsOne(x => x.AuditInfo, audit =>
        {
            audit.Property(x => x.CreatedAtUtc)
                .HasColumnName("CreatedAtUtc")
                .IsRequired();

            audit.Property(x => x.CreatedBy)
                .HasColumnName("CreatedBy")
                .HasMaxLength(200)
                .IsRequired();

            audit.Property(x => x.LastModifiedAtUtc)
                .HasColumnName("LastModifiedAtUtc");

            audit.Property(x => x.LastModifiedBy)
                .HasColumnName("LastModifiedBy")
                .HasMaxLength(200);
        });

        entity.Navigation(x => x.AuditInfo)
            .IsRequired();
    }
}
