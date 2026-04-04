using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SWFC.Domain.M100_System.M102_Organization.Entities;
using SWFC.Domain.M100_System.M102_Organization.ValueObjects;

namespace SWFC.Infrastructure.Persistence.Configurations.M100_System;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> entity)
    {
        entity.ToTable("Roles");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Name)
            .HasConversion(
                x => x.Value,
                v => RoleName.Create(v))
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(x => x.Description)
            .HasMaxLength(500)
            .IsRequired(false);

        entity.HasIndex(x => x.Name)
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