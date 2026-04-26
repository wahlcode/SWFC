using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SWFC.Domain.M700_Support.M701_BugTracking;

namespace SWFC.Infrastructure.Persistence.Configurations.M700_Support;

public sealed class BugConfiguration : IEntityTypeConfiguration<Bug>
{
    public void Configure(EntityTypeBuilder<Bug> entity)
    {
        entity.ToTable("Bugs");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Description)
            .HasColumnType("text")
            .IsRequired();

        entity.Property(x => x.Reproducibility)
            .HasColumnType("text")
            .IsRequired();

        entity.Property(x => x.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(32);

        entity.Property(x => x.ModuleReference)
            .HasMaxLength(50)
            .IsRequired(false);

        entity.Property(x => x.ObjectReference)
            .HasMaxLength(200)
            .IsRequired(false);

        entity.Property(x => x.HistoryLog)
            .HasColumnType("text")
            .IsRequired();

        entity.OwnsOne(x => x.AuditInfo, audit =>
        {
            audit.Property(a => a.CreatedAtUtc).IsRequired();
            audit.Property(a => a.CreatedBy).IsRequired();
            audit.Property(a => a.LastModifiedAtUtc).IsRequired(false);
            audit.Property(a => a.LastModifiedBy).HasMaxLength(200).IsRequired(false);
        });
    }
}
