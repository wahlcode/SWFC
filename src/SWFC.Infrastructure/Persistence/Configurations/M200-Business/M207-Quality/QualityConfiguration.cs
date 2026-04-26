using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SWFC.Domain.M200_Business.M207_Quality;

namespace SWFC.Infrastructure.Persistence.Configurations.M200_Business.M207_Quality;

public sealed class QualityCaseConfiguration : IEntityTypeConfiguration<QualityCase>
{
    public void Configure(EntityTypeBuilder<QualityCase> builder)
    {
        builder.ToTable("QualityCases");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.Source).HasConversion<int>().IsRequired();
        builder.Property(x => x.SourceReference).HasMaxLength(500).IsRequired(false);
        builder.Property(x => x.MachineId).IsRequired(false);
        builder.Property(x => x.MaintenanceOrderId).IsRequired(false);
        builder.Property(x => x.InspectionId).IsRequired(false);
        builder.Property(x => x.Priority).HasConversion<int>().IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.RootCause).HasMaxLength(2000).IsRequired(false);
        builder.Property(x => x.DueAtUtc).IsRequired(false);
        builder.Property(x => x.ResponsibleUserId).IsRequired(false);
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.ClosedAtUtc).IsRequired(false);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.InspectionId);
    }
}

public sealed class QualityActionConfiguration : IEntityTypeConfiguration<QualityAction>
{
    public void Configure(EntityTypeBuilder<QualityAction> builder)
    {
        builder.ToTable("QualityActions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.QualityCaseId).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Type).HasConversion<int>().IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.AssignedUserId).IsRequired(false);
        builder.Property(x => x.DueAtUtc).IsRequired(false);
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.ClosedAtUtc).IsRequired(false);
        builder.HasOne<QualityCase>().WithMany().HasForeignKey(x => x.QualityCaseId).OnDelete(DeleteBehavior.Restrict);
    }
}
