using SWFC.Domain.M200_Business.M213_Workforce;

namespace SWFC.Application.M200_Business.M213_Workforce;

public sealed record WorkforceAssignmentSummaryDto(
    Guid Id,
    Guid UserId,
    Guid? ShiftModelId,
    string TargetModule,
    string TargetLabel,
    string Activity,
    string Status,
    double ReportedHours);

public sealed class WorkforceWorkspaceService
{
    private readonly List<WorkforceAssignment> _assignments = new();

    public WorkforceWorkspaceService()
    {
        var assignment = new WorkforceAssignment(
            Guid.Parse("21300000-0000-0000-0000-000000000001"),
            Guid.Parse("10200000-0000-0000-0000-000000000003"),
            Guid.Parse("10200000-0000-0000-0000-000000000004"),
            new WorkforceTarget("M209", Guid.Parse("20900000-0000-0000-0000-000000000001"), "Leckageliste pruefen"),
            new DateTimeOffset(2026, 4, 26, 7, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 4, 26, 11, 0, 0, TimeSpan.Zero),
            "Projektaufgabe bearbeiten");
        assignment.Start();
        assignment.AddFeedback(new ActivityFeedback(
            new DateTimeOffset(2026, 4, 26, 7, 10, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 4, 26, 10, 40, 0, TimeSpan.Zero),
            "Aufgabe abgeschlossen"));
        _assignments.Add(assignment);
    }

    public IReadOnlyList<WorkforceAssignmentSummaryDto> GetAssignments()
    {
        return _assignments
            .Select(assignment => new WorkforceAssignmentSummaryDto(
                assignment.Id,
                assignment.UserId,
                assignment.ShiftModelId,
                assignment.Target.ModuleCode,
                assignment.Target.Label,
                assignment.Activity,
                assignment.Status.ToString(),
                assignment.Feedback.Sum(x => x.Duration.TotalHours)))
            .ToList();
    }
}
