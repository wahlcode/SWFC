using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SWFC.Domain.M800_Security.M806_AccessControl.Permissions;

namespace SWFC.Infrastructure.Persistence.Configurations.M800_Security;

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> entity)
    {
        entity.ToTable("Permissions");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(x => x.Description)
            .HasMaxLength(500)
            .IsRequired(false);

        entity.Property(x => x.Module)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(x => x.IsActive)
            .IsRequired();

        entity.HasIndex(x => x.Code)
            .IsUnique();

        entity.OwnsOne(x => x.AuditInfo, audit =>
        {
            audit.Property(a => a.CreatedAtUtc).IsRequired();
            audit.Property(a => a.CreatedBy).IsRequired();
            audit.Property(a => a.LastModifiedAtUtc).IsRequired(false);
            audit.Property(a => a.LastModifiedBy).HasMaxLength(200).IsRequired(false);
        });
    }
}
