using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SWFC.Domain.M700_Support.M706_SLA_Service_Levels;

namespace SWFC.Infrastructure.Persistence.Configurations.M700_Support;

public sealed class ServiceLevelConfiguration : IEntityTypeConfiguration<ServiceLevel>
{
    public void Configure(EntityTypeBuilder<ServiceLevel> entity)
    {
        entity.ToTable("ServiceLevels");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Priority)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(x => x.ResponseTime)
            .IsRequired();

        entity.Property(x => x.ProcessingTime)
            .IsRequired();

        entity.Property(x => x.UseForSupport)
            .IsRequired();

        entity.Property(x => x.UseForIncidentManagement)
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
