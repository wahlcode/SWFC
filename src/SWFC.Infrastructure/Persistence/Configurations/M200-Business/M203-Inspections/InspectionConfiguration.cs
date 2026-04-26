using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SWFC.Domain.M200_Business.M203_Inspections;

namespace SWFC.Infrastructure.Persistence.Configurations.M200_Business.M203_Inspections;

public sealed class InspectionPlanConfiguration : IEntityTypeConfiguration<InspectionPlan>
{
    public void Configure(EntityTypeBuilder<InspectionPlan> builder)
    {
        builder.ToTable("InspectionPlans");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.TargetType).HasConversion<int>().IsRequired();
        builder.Property(x => x.TargetId).IsRequired();
        builder.Property(x => x.ObjectType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.IntervalDays).IsRequired();
        builder.Property(x => x.NextDueAtUtc).IsRequired();
        builder.Property(x => x.ResponsibleUserId).IsRequired(false);
        builder.Property(x => x.DocumentReference).HasMaxLength(500).IsRequired(false);
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.ClosedAtUtc).IsRequired(false);
        builder.HasIndex(x => new { x.TargetType, x.TargetId });
    }
}

public sealed class InspectionConfiguration : IEntityTypeConfiguration<Inspection>
{
    public void Configure(EntityTypeBuilder<Inspection> builder)
    {
        builder.ToTable("Inspections");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.InspectionPlanId).IsRequired();
        builder.Property(x => x.TargetType).HasConversion<int>().IsRequired();
        builder.Property(x => x.TargetId).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Result).HasConversion<int>().IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.InspectorUserId).IsRequired(false);
        builder.Property(x => x.Notes).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.FollowUpReference).HasMaxLength(500).IsRequired(false);
        builder.Property(x => x.PerformedAtUtc).IsRequired();
        builder.HasOne<InspectionPlan>().WithMany().HasForeignKey(x => x.InspectionPlanId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.TargetType, x.TargetId });
    }
}
