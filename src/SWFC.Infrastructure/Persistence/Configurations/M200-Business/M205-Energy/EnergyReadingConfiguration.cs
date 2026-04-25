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
    }
}
