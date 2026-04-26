using SWFC.Domain.M200_Business.M203_Inspections;
using SWFC.Domain.M200_Business.M207_Quality;
using SWFC.Domain.M200_Business.M208_Safety;

namespace SWFC.Architecture.Tests.Rules;

public sealed class M203_M207_M208_V010CompletionTests
{
    [Fact]
    public void M203_Inspection_Result_Advances_Due_Date_And_Separates_Follow_Up_Reference()
    {
        var targetId = Guid.NewGuid();
        var plan = new InspectionPlan(
            Guid.NewGuid(),
            "Press safety inspection",
            InspectionTargetType.Machine,
            targetId,
            "Machine",
            30,
            Guid.NewGuid(),
            "DOC-INS-1");

        var inspection = new Inspection(
            Guid.NewGuid(),
            plan.Id,
            plan.TargetType,
            plan.TargetId,
            "Monthly check",
            InspectionResult.DefectFound,
            Guid.NewGuid(),
            "Guard needs adjustment.",
            "M207:quality-case");

        plan.MarkCycleCompleted(inspection.PerformedAtUtc);

        Assert.Equal(InspectionStatus.ActionRequired, inspection.Status);
        Assert.Equal("M207:quality-case", inspection.FollowUpReference);
        Assert.Equal(inspection.PerformedAtUtc.Date.AddDays(30), plan.NextDueAtUtc);
        Assert.True(plan.IsActive);
    }

    [Fact]
    public void M207_Quality_Case_Tracks_Root_Cause_Actions_And_Escalation()
    {
        var qualityCase = new QualityCase(
            Guid.NewGuid(),
            "Inspection defect",
            "Measured value outside tolerance.",
            QualityCaseSource.Inspection,
            "M203:inspection",
            Guid.NewGuid(),
            null,
            Guid.NewGuid(),
            QualityPriority.Critical,
            DateTime.UtcNow.AddDays(-1),
            Guid.NewGuid());

        qualityCase.StartRootCauseAnalysis("Fixture wear.");
        var action = new QualityAction(
            Guid.NewGuid(),
            qualityCase.Id,
            "Replace fixture",
            QualityActionType.Corrective,
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(2));

        Assert.Equal(QualityCaseStatus.InProgress, qualityCase.Status);
        Assert.Equal("Fixture wear.", qualityCase.RootCause);
        Assert.Equal((int)QualityPriority.Critical, qualityCase.EscalationLevel(DateTime.UtcNow));
        Assert.Equal(QualityActionStatus.Planned, action.Status);
    }

    [Fact]
    public void M208_Safety_Assessment_Calculates_Risk_And_Permit_Decides_Action()
    {
        var assessment = new SafetyAssessment(
            Guid.NewGuid(),
            "Remote maintenance intervention",
            SafetyTargetType.Machine,
            Guid.NewGuid(),
            "Unexpected startup",
            3,
            5,
            "Lockout and second-person approval required.",
            Guid.NewGuid(),
            "DOC-SAFE-1",
            Guid.NewGuid());

        var permit = new SafetyPermit(
            Guid.NewGuid(),
            assessment.Id,
            assessment.Activity,
            DateTime.UtcNow.AddHours(2),
            "Only after lockout confirmation.",
            Guid.NewGuid());

        Assert.Equal(15, assessment.RiskScore);
        Assert.Equal(SafetyAssessmentStatus.Active, assessment.Status);
        Assert.True(permit.AllowsAction(DateTime.UtcNow));
        Assert.Equal(SafetyPermitStatus.Approved, permit.Status);
    }
}
