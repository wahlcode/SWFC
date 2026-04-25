using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SWFC.Domain.M100_System.M102_Organization.CostCenters;
using SWFC.Domain.M100_System.M102_Organization.ShiftModels;
using SWFC.Domain.M100_System.M102_Organization.Users;

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

        entity.Property(x => x.Username)
            .HasConversion(
                x => x.Value,
                v => Username.Create(v))
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(x => x.DisplayName)
            .HasConversion(
                x => x.Value,
                v => UserDisplayName.Create(v))
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(x => x.EmployeeNumber)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(x => x.BusinessEmail)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(x => x.BusinessPhone)
            .IsRequired()
            .HasMaxLength(50);

        entity.Property(x => x.Plant)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(x => x.Location)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(x => x.Team)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(x => x.CostCenterId)
            .IsRequired(false);

        entity.Property(x => x.ShiftModelId)
            .IsRequired(false);

        entity.Property(x => x.JobFunction)
            .IsRequired()
            .HasMaxLength(150);

        entity.Property(x => x.PreferredCultureName)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("de-DE");

        entity.Property(x => x.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(32);

        entity.Property(x => x.UserType)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(32);

        entity.Property(x => x.IsActive)
            .IsRequired();

        entity.HasIndex(x => x.IdentityKey)
            .IsUnique();

        entity.HasIndex(x => x.Username)
            .IsUnique();

        entity.HasOne<CostCenter>()
            .WithMany()
            .HasForeignKey(x => x.CostCenterId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne<ShiftModel>()
            .WithMany()
            .HasForeignKey(x => x.ShiftModelId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.OwnsOne(x => x.AuditInfo, audit =>
        {
            audit.Property(a => a.CreatedAtUtc).IsRequired();
            audit.Property(a => a.CreatedBy).IsRequired();
            audit.Property(a => a.LastModifiedAtUtc).IsRequired(false);
            audit.Property(a => a.LastModifiedBy).HasMaxLength(200).IsRequired(false);
        });

        entity.HasMany(x => x.OrganizationUnits)
            .WithOne()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}