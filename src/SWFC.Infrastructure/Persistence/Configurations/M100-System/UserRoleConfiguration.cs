using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SWFC.Domain.M100_System.M102_Organization.Entities;

namespace SWFC.Infrastructure.Persistence.Configurations.M100_System;

public sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> entity)
    {
        entity.ToTable("UserRoles");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.UserId)
            .IsRequired();

        entity.Property(x => x.RoleId)
            .IsRequired();

        entity.HasIndex(x => new { x.UserId, x.RoleId })
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