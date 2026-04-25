using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SWFC.Domain.M200_Business.M206_Purchasing.PurchaseRequirements;

namespace SWFC.Infrastructure.Persistence.Configurations.M200_Business.M206_Purchasing;

public sealed class PurchaseRequirementConfiguration : IEntityTypeConfiguration<PurchaseRequirement>
{
    public void Configure(EntityTypeBuilder<PurchaseRequirement> builder)
    {
        builder.ToTable("PurchaseRequirements");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.RequiredItem)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Quantity)
            .IsRequired();

        builder.Property(x => x.Unit)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.SourceType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.SourceReferenceId)
            .IsRequired(false);

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.DeactivatedAtUtc)
            .IsRequired(false);

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.SourceReferenceId);
    }
}