using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SWFC.Infrastructure.M100_System.M107_SetupDeployment.Entities;

namespace SWFC.Infrastructure.Persistence.Configurations.M100_System;

public sealed class SetupStateConfiguration : IEntityTypeConfiguration<SetupState>
{
    public void Configure(EntityTypeBuilder<SetupState> entity)
    {
        entity.ToTable("SetupStates");

        entity.HasKey(x => x.Key);

        entity.Property(x => x.Key)
            .HasMaxLength(50)
            .IsRequired();

        entity.Property(x => x.IsConfigured)
            .IsRequired();

        entity.Property(x => x.SetupCompleted)
            .IsRequired();

        entity.Property(x => x.DatabaseInitialized)
            .IsRequired();

        entity.Property(x => x.SetupInProgress)
            .IsRequired();

        entity.Property(x => x.LastCheckedAtUtc)
            .IsRequired();

        entity.Property(x => x.CompletedAtUtc)
            .IsRequired(false);

        entity.Property(x => x.LastFailure)
            .HasMaxLength(1000)
            .IsRequired(false);
    }
}
