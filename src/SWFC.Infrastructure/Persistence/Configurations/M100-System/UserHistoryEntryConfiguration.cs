using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SWFC.Domain.M100_System.M102_Organization.Users;

namespace SWFC.Infrastructure.Persistence.Configurations.M100_System;

public sealed class UserHistoryEntryConfiguration : IEntityTypeConfiguration<UserHistoryEntry>
{
    public void Configure(EntityTypeBuilder<UserHistoryEntry> entity)
    {
        entity.ToTable("UserHistoryEntries");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.UserId)
            .IsRequired();

        entity.Property(x => x.ChangeType)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(64);

        entity.Property(x => x.Summary)
            .IsRequired()
            .HasMaxLength(250);

        entity.Property(x => x.SnapshotJson)
            .IsRequired()
            .HasColumnType("text");

        entity.Property(x => x.Reason)
            .IsRequired()
            .HasMaxLength(500);

        entity.Property(x => x.ChangedAtUtc)
            .IsRequired();

        entity.Property(x => x.ChangedByUserId)
            .IsRequired()
            .HasMaxLength(200);

        entity.HasIndex(x => x.UserId);
        entity.HasIndex(x => x.ChangedAtUtc);

        entity.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
