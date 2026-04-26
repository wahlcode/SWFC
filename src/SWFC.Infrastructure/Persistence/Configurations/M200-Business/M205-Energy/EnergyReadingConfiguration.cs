using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SWFC.Domain.M200_Business.M205_Energy.EnergyReadings;

namespace SWFC.Infrastructure.Persistence.Configurations.M200_Business.M205_Energy;

public sealed class EnergyReadingConfiguration : IEntityTypeConfiguration<EnergyReading>
{
    public void Configure(EntityTypeBuilder<EnergyReading> builder)
    {
        builder.ToTable("EnergyReadings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.MeterId)
            .IsRequired();

        builder.Property(x => x.Source)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.CapturedByUserId)
            .HasMaxLength(200)
            .IsRequired(false);

        builder.Property(x => x.OfflineCaptureId)
            .IsRequired(false);

        builder.Property(x => x.CapturedOfflineAtUtc)
            .IsRequired(false);

        builder.Property(x => x.SyncedAtUtc)
            .IsRequired(false);

        builder.Property(x => x.PlausibilityStatus)
            .HasConversion<int>()
            .IsRequired();

        builder.OwnsOne(x => x.Date, date =>
        {
            date.Property(v => v.Value)
                .HasColumnName("ReadingDate")
                .IsRequired();
        });

        builder.OwnsOne(x => x.Value, value =>
        {
            value.Property(v => v.Value)
                .HasColumnName("ReadingValue")
                .HasPrecision(18, 3)
                .IsRequired();
        });

        builder.OwnsOne(x => x.CaptureContext, context =>
        {
            context.Property(v => v!.Value)
                .HasColumnName("CaptureContext")
                .HasMaxLength(500)
                .IsRequired(false);
        });

        builder.OwnsOne(x => x.RfidTag, rfidTag =>
        {
            rfidTag.Property(v => v!.Value)
                .HasColumnName("RfidTag")
                .HasMaxLength(100)
                .IsRequired(false);
        });

        builder.OwnsOne(x => x.RfidExceptionReason, reason =>
        {
            reason.Property(v => v!.Value)
                .HasColumnName("RfidExceptionReason")
                .HasMaxLength(500)
                .IsRequired(false);
        });

        builder.OwnsOne(x => x.PlausibilityNote, note =>
        {
            note.Property(v => v!.Value)
                .HasColumnName("PlausibilityNote")
                .HasMaxLength(500)
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

        builder.HasIndex(x => new { x.MeterId });
        builder.HasIndex(x => x.OfflineCaptureId)
            .IsUnique()
            .HasFilter("\"OfflineCaptureId\" IS NOT NULL");
    }
}
