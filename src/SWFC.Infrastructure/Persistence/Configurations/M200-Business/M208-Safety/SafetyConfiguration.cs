using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SWFC.Domain.M200_Business.M208_Safety;

namespace SWFC.Infrastructure.Persistence.Configurations.M200_Business.M208_Safety;

public sealed class SafetyAssessmentConfiguration : IEntityTypeConfiguration<SafetyAssessment>
{
    public void Configure(EntityTypeBuilder<SafetyAssessment> builder)
    {
        builder.ToTable("SafetyAssessments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Activity).HasMaxLength(200).IsRequired();
        builder.Property(x => x.TargetType).HasConversion<int>().IsRequired();
        builder.Property(x => x.TargetId).IsRequired();
        builder.Property(x => x.Hazard).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.Likelihood).IsRequired();
        builder.Property(x => x.Severity).IsRequired();
        builder.Ignore(x => x.RiskScore);
        builder.Property(x => x.RequiredMeasures).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.ResponsibleUserId).IsRequired(false);
        builder.Property(x => x.DocumentReference).HasMaxLength(500).IsRequired(false);
        builder.Property(x => x.QualityCaseId).IsRequired(false);
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.ClosedAtUtc).IsRequired(false);
        builder.HasIndex(x => new { x.TargetType, x.TargetId });
        builder.HasIndex(x => x.QualityCaseId);
    }
}

public sealed class SafetyPermitConfiguration : IEntityTypeConfiguration<SafetyPermit>
{
    public void Configure(EntityTypeBuilder<SafetyPermit> builder)
    {
        builder.ToTable("SafetyPermits");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AssessmentId).IsRequired();
        builder.Property(x => x.Activity).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ValidUntilUtc).IsRequired();
        builder.Property(x => x.Restriction).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.ApprovedByUserId).IsRequired(false);
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.HasOne<SafetyAssessment>().WithMany().HasForeignKey(x => x.AssessmentId).OnDelete(DeleteBehavior.Restrict);
    }
}
