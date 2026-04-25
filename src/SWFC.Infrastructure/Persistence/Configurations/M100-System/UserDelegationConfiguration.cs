using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SWFC.Domain.M100_System.M102_Organization.Users.Delegations;

namespace SWFC.Infrastructure.Persistence.Configurations.M100_System;

public sealed class UserDelegationConfiguration : IEntityTypeConfiguration<UserDelegation>
{
    public void Configure(EntityTypeBuilder<UserDelegation> builder)
    {
        builder.ToTable("M102UserDelegations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.DelegateUserId)
            .IsRequired();

        builder.Property(x => x.DelegationType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ValidFromUtc)
            .IsRequired();

        builder.Property(x => x.ValidToUtc);

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

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.DelegateUserId);
        builder.HasIndex(x => new { x.UserId, x.IsActive, x.ValidFromUtc, x.ValidToUtc });
    }
}