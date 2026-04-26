using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SWFC.Domain.M200_Business.M202_Maintenance.MaintenanceOrders;

namespace SWFC.Infrastructure.Persistence.Configurations.M200_Business;

public sealed class MaintenanceOrderConfiguration : IEntityTypeConfiguration<MaintenanceOrder>
{
    public void Configure(EntityTypeBuilder<MaintenanceOrder> builder)
    {
        builder.ToTable("MaintenanceOrders");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Priority)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.TargetType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.TargetId)
            .IsRequired();

        builder.Property(x => x.MaintenancePlanId)
            .IsRequired(false);

        builder.Property(x => x.PlannedStartUtc)
            .IsRequired(false);

        builder.Property(x => x.PlannedEndUtc)
            .IsRequired(false);

        builder.Property(x => x.StartedAtUtc)
            .IsRequired(false);

        builder.Property(x => x.CompletedAtUtc)
            .IsRequired(false);

        builder.Property(x => x.DueAtUtc)
            .IsRequired(false);

        builder.OwnsOne(x => x.Number, owned =>
        {
            owned.Property(p => p.Value)
                .HasColumnName("Number")
                .HasMaxLength(50)
                .IsRequired();
        });

        builder.Navigation(x => x.Number).IsRequired();

        builder.OwnsOne(x => x.Title, owned =>
        {
            owned.Property(p => p.Value)
                .HasColumnName("Title")
                .HasMaxLength(200)
                .IsRequired();
        });

        builder.Navigation(x => x.Title).IsRequired();

        builder.OwnsOne(x => x.Description, owned =>
        {
            owned.Property(p => p.Value)
                .HasColumnName("Description")
                .HasMaxLength(2000)
                .IsRequired();
        });

        builder.Navigation(x => x.Description).IsRequired();

        builder.OwnsOne(x => x.AuditInfo, audit =>
        {
            audit.Property(a => a.CreatedAtUtc)
                .HasColumnName("CreatedAtUtc")
                .IsRequired();

            audit.Property(a => a.CreatedBy)
                .HasColumnName("CreatedBy")
                .HasMaxLength(200)
                .IsRequired();

            audit.Property(a => a.LastModifiedAtUtc)
                .HasColumnName("LastModifiedAtUtc")
                .IsRequired(false);

            audit.Property(a => a.LastModifiedBy)
                .HasColumnName("LastModifiedBy")
                .HasMaxLength(200)
                .IsRequired(false);
        });

        builder.Navigation(x => x.AuditInfo).IsRequired();

        builder.OwnsMany(x => x.Materials, materials =>
        {
            materials.ToTable("MaintenanceOrderMaterials");

            materials.WithOwner()
                .HasForeignKey(x => x.MaintenanceOrderId);

            materials.HasKey(x => x.Id);

            materials.Property(x => x.Id)
                .ValueGeneratedNever();

            materials.Property(x => x.MaintenanceOrderId)
                .IsRequired();

            materials.Property(x => x.ItemId)
                .IsRequired();

            materials.Property(x => x.Quantity)
                .IsRequired();

            materials.OwnsOne(x => x.AuditInfo, audit =>
            {
                audit.Property(a => a.CreatedAtUtc)
                    .HasColumnName("CreatedAtUtc")
                    .IsRequired();

                audit.Property(a => a.CreatedBy)
                    .HasColumnName("CreatedBy")
                    .HasMaxLength(200)
                    .IsRequired();

                audit.Property(a => a.LastModifiedAtUtc)
                    .HasColumnName("LastModifiedAtUtc")
                    .IsRequired(false);

                audit.Property(a => a.LastModifiedBy)
                    .HasColumnName("LastModifiedBy")
                    .HasMaxLength(200)
                    .IsRequired(false);
            });

            materials.Navigation(x => x.AuditInfo).IsRequired();
        });

        builder.Navigation(x => x.Materials)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
