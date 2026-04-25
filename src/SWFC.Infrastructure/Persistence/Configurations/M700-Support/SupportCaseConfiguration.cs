using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SWFC.Domain.M700_Support.M701_BugTracking;
using SWFC.Domain.M700_Support.M703_SupportCases;
using SWFC.Domain.M700_Support.M704_Incident_Management;

namespace SWFC.Infrastructure.Persistence.Configurations.M700_Support;

public sealed class SupportCaseConfiguration : IEntityTypeConfiguration<SupportCase>
{
    public void Configure(EntityTypeBuilder<SupportCase> entity)
    {
        entity.ToTable("SupportCases");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.UserRequest)
            .HasColumnType("text")
            .IsRequired();

        entity.Property(x => x.ProblemDescription)
            .HasColumnType("text")
            .IsRequired();

        entity.Property(x => x.TriggeredBugId)
            .IsRequired(false);

        entity.Property(x => x.TriggeredIncidentId)
            .IsRequired(false);

        entity.HasOne<Bug>()
            .WithMany()
            .HasForeignKey(x => x.TriggeredBugId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne<Incident>()
            .WithMany()
            .HasForeignKey(x => x.TriggeredIncidentId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.OwnsOne(x => x.AuditInfo, audit =>
        {
            audit.Property(a => a.CreatedAtUtc).IsRequired();
            audit.Property(a => a.CreatedBy).IsRequired();
            audit.Property(a => a.LastModifiedAtUtc).IsRequired(false);
            audit.Property(a => a.LastModifiedBy).HasMaxLength(200).IsRequired(false);
        });
    }
}
