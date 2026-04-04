using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SWFC.Domain.M100_System.M102_Organization.Entities;

namespace SWFC.Infrastructure.Persistence.Configurations.M100_System;

public sealed class UserOrganizationUnitConfiguration : IEntityTypeConfiguration<UserOrganizationUnit>
{
    public void Configure(EntityTypeBuilder<UserOrganizationUnit> entity)
    {
        entity.ToTable("UserOrganizationUnits");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.UserId)
            .IsRequired();

        entity.Property(x => x.OrganizationUnitId)
            .IsRequired();

        entity.Property(x => x.IsPrimary)
            .IsRequired();

        entity.HasIndex(x => new { x.UserId, x.OrganizationUnitId })
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