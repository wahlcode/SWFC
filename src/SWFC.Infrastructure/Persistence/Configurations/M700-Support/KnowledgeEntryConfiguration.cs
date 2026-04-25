using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SWFC.Domain.M700_Support.M705_Knowledge_Base;

namespace SWFC.Infrastructure.Persistence.Configurations.M700_Support;

public sealed class KnowledgeEntryConfiguration : IEntityTypeConfiguration<KnowledgeEntry>
{
    public void Configure(EntityTypeBuilder<KnowledgeEntry> entity)
    {
        entity.ToTable("KnowledgeEntries");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Type)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(32);

        entity.Property(x => x.Content)
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
