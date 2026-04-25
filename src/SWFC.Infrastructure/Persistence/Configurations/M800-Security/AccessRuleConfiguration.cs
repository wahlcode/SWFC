using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SWFC.Domain.M800_Security.M801_Access;

namespace SWFC.Infrastructure.Persistence.Configurations.M800_Security;

public sealed class AccessRuleConfiguration : IEntityTypeConfiguration<AccessRule>
{
    public void Configure(EntityTypeBuilder<AccessRule> entity)
    {
        entity.ToTable("AccessRules");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.TargetType)
            .HasConversion<int>()
            .IsRequired();

        entity.Property(x => x.TargetId)
            .HasMaxLength(200)
            .IsRequired();

        entity.Property(x => x.SubjectType)
            .HasConversion<int>()
            .IsRequired();

        entity.Property(x => x.SubjectId)
            .HasMaxLength(200)
            .IsRequired();

        entity.Property(x => x.Mode)
            .HasConversion<int>()
            .IsRequired();

        entity.Property(x => x.IsActive)
            .IsRequired();

        entity.HasIndex(x => new
        {
            x.TargetType,
            x.TargetId,
            x.SubjectType,
            x.SubjectId
        }).IsUnique();

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
