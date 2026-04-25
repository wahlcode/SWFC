using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SWFC.Domain.M100_System.M102_Organization.Users;
using SWFC.Infrastructure.M100_System.M103_Authentication.Entities;

namespace SWFC.Infrastructure.Persistence.Configurations.M103_Authentication;

public sealed class LocalCredentialConfiguration : IEntityTypeConfiguration<LocalCredential>
{
    public void Configure(EntityTypeBuilder<LocalCredential> entity)
    {
        entity.ToTable("LocalCredentials");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.UserId)
            .IsRequired();

        entity.Property(x => x.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        entity.Property(x => x.IsActive)
            .IsRequired();

        entity.Property(x => x.FailedAttempts)
            .IsRequired();

        entity.Property(x => x.LockoutUntilUtc)
            .IsRequired(false);

        entity.Property(x => x.LastPasswordChangedAtUtc)
            .IsRequired();

        entity.HasIndex(x => x.UserId)
            .IsUnique();

        entity.HasOne<User>()
            .WithOne()
            .HasForeignKey<LocalCredential>(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
