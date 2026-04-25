using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SWFC.Domain.M100_System.M102_Organization.Users.ExternalPersons;

namespace SWFC.Infrastructure.Persistence.Configurations.M100_System;

public sealed class ExternalPersonConfiguration : IEntityTypeConfiguration<ExternalPerson>
{
    public void Configure(EntityTypeBuilder<ExternalPerson> builder)
    {
        builder.ToTable("M102ExternalPersons");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DisplayName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.CompanyName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Email)
            .HasMaxLength(200);

        builder.Property(x => x.Phone)
            .HasMaxLength(100);

        builder.Property(x => x.Function)
            .HasMaxLength(150);

        builder.Property(x => x.OrganizationUnitId);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.OwnsOne(x => x.AuditInfo, audit =>
        {
            audit.Property(x => x.CreatedAtUtc)
                .HasColumnName("CreatedAtUtc")
                .IsRequired();

            audit.Property(x => x.CreatedBy)
                .HasColumnName("CreatedBy")
                .HasMaxLength(200)
                .IsRequired();

            audit.Property(x => x.LastModifiedAtUtc)
                .HasColumnName("LastModifiedAtUtc");

            audit.Property(x => x.LastModifiedBy)
                .HasColumnName("LastModifiedBy")
                .HasMaxLength(200);
        });

        builder.HasIndex(x => x.DisplayName);
        builder.HasIndex(x => x.CompanyName);
        builder.HasIndex(x => new { x.OrganizationUnitId, x.IsActive });
    }
}