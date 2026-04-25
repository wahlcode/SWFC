using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SWFC.Domain.M200_Business.M206_Purchasing.PurchaseRequirements;
using SWFC.Domain.M200_Business.M206_Purchasing.RequestForQuotations;
using SWFC.Domain.M200_Business.M206_Purchasing.Suppliers;

namespace SWFC.Infrastructure.Persistence.Configurations.M200_Business.M206_Purchasing;

public sealed class RequestForQuotationConfiguration : IEntityTypeConfiguration<RequestForQuotation>
{
    public void Configure(EntityTypeBuilder<RequestForQuotation> builder)
    {
        builder.ToTable("RequestForQuotations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PurchaseRequirementId)
            .IsRequired();

        builder.Property(x => x.SupplierId)
            .IsRequired();

        builder.Property(x => x.RequestedAtUtc)
            .IsRequired();

        builder.Property(x => x.ResponseDueAtUtc)
            .IsRequired(false);

        builder.Property(x => x.IsClosed)
            .IsRequired();

        builder.HasOne<PurchaseRequirement>()
            .WithMany()
            .HasForeignKey(x => x.PurchaseRequirementId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Supplier>()
            .WithMany()
            .HasForeignKey(x => x.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.PurchaseRequirementId);
        builder.HasIndex(x => x.SupplierId);
    }
}