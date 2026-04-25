using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SWFC.Domain.M200_Business.M204_Inventory.Items;
using SWFC.Domain.M200_Business.M204_Inventory.Locations;
using SWFC.Domain.M200_Business.M204_Inventory.Stock;
using SWFC.Domain.M200_Business.M206_Purchasing.GoodsReceipts;
using SWFC.Domain.M200_Business.M206_Purchasing.PurchaseOrders;

namespace SWFC.Infrastructure.Persistence.Configurations.M200_Business.M206_Purchasing;

public sealed class GoodsReceiptConfiguration : IEntityTypeConfiguration<GoodsReceipt>
{
    public void Configure(EntityTypeBuilder<GoodsReceipt> builder)
    {
        builder.ToTable("GoodsReceipts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PurchaseOrderId)
            .IsRequired();

        builder.Property(x => x.InventoryItemId)
            .IsRequired();

        builder.Property(x => x.LocationId)
            .IsRequired();

        builder.Property(x => x.Bin)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(x => x.Quantity)
            .IsRequired();

        builder.Property(x => x.Unit)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ReceivedAtUtc)
            .IsRequired();

        builder.Property(x => x.InventoryBookingStatus)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.InventoryStockMovementId)
            .IsRequired(false);

        builder.Property(x => x.InventoryBookingMessage)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.HasOne<PurchaseOrder>()
            .WithMany()
            .HasForeignKey(x => x.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<InventoryItem>()
            .WithMany()
            .HasForeignKey(x => x.InventoryItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Location>()
            .WithMany()
            .HasForeignKey(x => x.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<StockMovement>()
            .WithMany()
            .HasForeignKey(x => x.InventoryStockMovementId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.PurchaseOrderId);
        builder.HasIndex(x => x.InventoryItemId);
        builder.HasIndex(x => x.LocationId);
        builder.HasIndex(x => x.InventoryStockMovementId);
        builder.HasIndex(x => x.InventoryBookingStatus);
        builder.HasIndex(x => x.ReceivedAtUtc);
    }
}
