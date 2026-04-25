using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SWFC.Domain.M700_Support.M704_Incident_Management;

namespace SWFC.Infrastructure.Persistence.Configurations.M700_Support;

public sealed class IncidentConfiguration : IEntityTypeConfiguration<Incident>
{
    public void Configure(EntityTypeBuilder<Incident> entity)
    {
        entity.ToTable("Incidents");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Category)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(32);

        entity.Property(x => x.Description)
            .HasColumnType("text")
            .IsRequired();

        entity.Property(x => x.Escalation)
            .HasColumnType("text")
            .IsRequired();

        entity.Property(x => x.ReactionControl)
            .HasColumnType("text")
            .IsRequired();

        entity.Property(x => x.NotificationReference)
            .HasMaxLength(200)
            .IsRequired(false);

        entity.Property(x => x.RuntimeReference)
            .HasMaxLength(200)
            .IsRequired(false);

        entity.OwnsOne(x => x.AuditInfo, audit =>
        {
            audit.Property(a => a.CreatedAtUtc).IsRequired();
            audit.Property(a => a.CreatedBy).IsRequired();
            audit.Property(a => a.LastModifiedAtUtc).IsRequired(false);
            audit.Property(a => a.LastModifiedBy).HasMaxLength(200).IsRequired(false);
        });
    }
}
