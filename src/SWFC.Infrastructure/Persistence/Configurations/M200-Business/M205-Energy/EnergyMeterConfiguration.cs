using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SWFC.Domain.M200_Business.M205_Energy.EnergyMeters;

namespace SWFC.Infrastructure.Persistence.Configurations.M200_Business.M205_Energy;

public sealed class EnergyMeterConfiguration : IEntityTypeConfiguration<EnergyMeter>
{
    public void Configure(EntityTypeBuilder<EnergyMeter> builder)
    {
        builder.ToTable("EnergyMeters");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.MediumType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.IsManualEntryEnabled)
            .IsRequired();

        builder.Property(x => x.IsExternalImportEnabled)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.SupportsOfflineCapture)
            .IsRequired();

        builder.Property(x => x.ParentMeterId)
            .IsRequired(false);

        builder.Property(x => x.MachineId)
            .IsRequired(false);

        builder.OwnsOne(x => x.Name, name =>
        {
            name.Property(v => v.Value)
                .HasColumnName("Name")
                .HasMaxLength(200)
                .IsRequired();
        });

        builder.OwnsOne(x => x.Unit, unit =>
        {
            unit.Property(v => v.Value)
                .HasColumnName("Unit")
                .HasMaxLength(50)
                .IsRequired();
        });

        builder.OwnsOne(x => x.MediumName, mediumName =>
        {
            mediumName.Property(v => v.Value)
                .HasColumnName("MediumName")
                .HasMaxLength(100)
                .IsRequired();
        });

        builder.OwnsOne(x => x.ExternalSystem, externalSystem =>
        {
            externalSystem.Property(v => v!.Value)
                .HasColumnName("ExternalSystem")
                .HasMaxLength(100)
                .IsRequired(false);
        });

        builder.OwnsOne(x => x.RfidTag, rfidTag =>
        {
            rfidTag.Property(v => v!.Value)
                .HasColumnName("RfidTag")
                .HasMaxLength(100)
                .IsRequired(false);
        });

        builder.OwnsOne(x => x.AuditInfo, audit =>
        {
            audit.Property(v => v.CreatedAtUtc)
                .HasColumnName("CreatedAtUtc")
                .IsRequired();

            audit.Property(v => v.CreatedBy)
                .HasColumnName("CreatedBy")
                .HasMaxLength(200)
                .IsRequired();

            audit.Property(v => v.LastModifiedAtUtc)
                .HasColumnName("LastModifiedAtUtc")
                .IsRequired(false);

            audit.Property(v => v.LastModifiedBy)
                .HasColumnName("LastModifiedBy")
                .HasMaxLength(200)
                .IsRequired(false);
        });

        builder.HasOne<EnergyMeter>()
            .WithMany()
            .HasForeignKey(x => x.ParentMeterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
