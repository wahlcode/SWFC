using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SWFC.Domain.M100_System.M102_Organization.Entities;
using SWFC.Domain.M100_System.M102_Organization.ValueObjects;

namespace SWFC.Infrastructure.Persistence.Configurations.M100_System;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.ToTable("Users");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.IdentityKey)
            .HasConversion(
                x => x.Value,
                v => UserIdentityKey.Create(v))
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(x => x.DisplayName)
            .HasConversion(
                x => x.Value,
                v => UserDisplayName.Create(v))
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(x => x.IsActive)
            .IsRequired();

        entity.HasIndex(x => x.IdentityKey)
            .IsUnique();

        entity.OwnsOne(x => x.AuditInfo, audit =>
        {
            audit.Property(a => a.CreatedAtUtc).IsRequired();
            audit.Property(a => a.CreatedBy).IsRequired();
            audit.Property(a => a.LastModifiedAtUtc).IsRequired(false);
            audit.Property(a => a.LastModifiedBy).HasMaxLength(200).IsRequired(false);
        });

        entity.HasMany(x => x.Roles)
            .WithOne()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasMany(x => x.OrganizationUnits)
            .WithOne()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}