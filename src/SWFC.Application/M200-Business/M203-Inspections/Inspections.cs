using SWFC.Domain.M200_Business.M203_Inspections;

namespace SWFC.Application.M200_Business.M203_Inspections;

public interface IInspectionPlanRepository
{
    Task AddAsync(InspectionPlan plan, CancellationToken cancellationToken = default);
    Task<InspectionPlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InspectionPlanListItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IInspectionRepository
{
    Task AddAsync(Inspection inspection, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InspectionListItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public sealed record CreateInspectionPlanRequest(
    string Name,
    InspectionTargetType TargetType,
    Guid TargetId,
    string ObjectType,
    int IntervalDays,
    Guid? ResponsibleUserId,
    string? DocumentReference);

public sealed record CreateInspectionRequest(
    Guid InspectionPlanId,
    string Title,
    InspectionResult Result,
    Guid? InspectorUserId,
    string? Notes,
    string? FollowUpReference);

public sealed record InspectionPlanListItem(
    Guid Id,
    string Name,
    InspectionTargetType TargetType,
    Guid TargetId,
    string ObjectType,
    int IntervalDays,
    DateTime NextDueAtUtc,
    Guid? ResponsibleUserId,
    string? DocumentReference,
    bool IsActive);

public sealed record InspectionListItem(
    Guid Id,
    Guid InspectionPlanId,
    InspectionTargetType TargetType,
    Guid TargetId,
    string Title,
    InspectionResult Result,
    InspectionStatus Status,
    Guid? InspectorUserId,
    string Notes,
    string? FollowUpReference,
    DateTime PerformedAtUtc);

public sealed class CreateInspectionPlan
{
    private readonly IInspectionPlanRepository _plans;

    public CreateInspectionPlan(IInspectionPlanRepository plans)
    {
        _plans = plans;
    }

    public async Task<Guid> ExecuteAsync(CreateInspectionPlanRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var plan = new InspectionPlan(
            Guid.NewGuid(),
            request.Name,
            request.TargetType,
            request.TargetId,
            request.ObjectType,
            request.IntervalDays,
            request.ResponsibleUserId,
            request.DocumentReference);

        await _plans.AddAsync(plan, cancellationToken);
        await _plans.SaveChangesAsync(cancellationToken);
        return plan.Id;
    }
}

public sealed class CreateInspection
{
    private readonly IInspectionPlanRepository _plans;
    private readonly IInspectionRepository _inspections;

    public CreateInspection(IInspectionPlanRepository plans, IInspectionRepository inspections)
    {
        _plans = plans;
        _inspections = inspections;
    }

    public async Task<Guid> ExecuteAsync(CreateInspectionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var plan = await _plans.GetByIdAsync(request.InspectionPlanId, cancellationToken)
            ?? throw new InvalidOperationException("Inspection plan was not found.");

        var inspection = new Inspection(
            Guid.NewGuid(),
            plan.Id,
            plan.TargetType,
            plan.TargetId,
            request.Title,
            request.Result,
            request.InspectorUserId,
            request.Notes,
            request.FollowUpReference);

        plan.MarkCycleCompleted(inspection.PerformedAtUtc);
        await _inspections.AddAsync(inspection, cancellationToken);
        await _inspections.SaveChangesAsync(cancellationToken);
        await _plans.SaveChangesAsync(cancellationToken);
        return inspection.Id;
    }
}

public sealed class GetInspectionPlans
{
    private readonly IInspectionPlanRepository _plans;

    public GetInspectionPlans(IInspectionPlanRepository plans)
    {
        _plans = plans;
    }

    public Task<IReadOnlyList<InspectionPlanListItem>> ExecuteAsync(CancellationToken cancellationToken = default)
        => _plans.GetAllAsync(cancellationToken);
}

public sealed class GetInspections
{
    private readonly IInspectionRepository _inspections;

    public GetInspections(IInspectionRepository inspections)
    {
        _inspections = inspections;
    }

    public Task<IReadOnlyList<InspectionListItem>> ExecuteAsync(CancellationToken cancellationToken = default)
        => _inspections.GetAllAsync(cancellationToken);
}
