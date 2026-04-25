using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SWFC.Domain.M800_Security.M806_AccessControl.Permissions;
using SWFC.Domain.M800_Security.M806_AccessControl.RolePermissions;
using SWFC.Domain.M800_Security.M806_AccessControl.Roles;

namespace SWFC.Infrastructure.Persistence.Configurations.M800_Security;

public sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> entity)
    {
        entity.ToTable("RolePermissions");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.RoleId)
            .IsRequired();

        entity.Property(x => x.PermissionId)
            .IsRequired();

        entity.Property(x => x.IsActive)
            .IsRequired();

        entity.HasIndex(x => new { x.RoleId, x.PermissionId })
            .IsUnique();

        entity.HasOne<Role>()
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne<Permission>()
            .WithMany()
            .HasForeignKey(x => x.PermissionId)
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
