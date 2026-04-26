using SWFC.Domain.M200_Business.M207_Quality;

namespace SWFC.Application.M200_Business.M207_Quality;

public interface IQualityCaseRepository
{
    Task AddAsync(QualityCase qualityCase, CancellationToken cancellationToken = default);
    Task<QualityCase?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<QualityCaseListItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IQualityActionRepository
{
    Task AddAsync(QualityAction action, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<QualityActionListItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public sealed record CreateQualityCaseRequest(
    string Title,
    string Description,
    QualityCaseSource Source,
    string? SourceReference,
    Guid? MachineId,
    Guid? MaintenanceOrderId,
    Guid? InspectionId,
    QualityPriority Priority,
    DateTime? DueAtUtc,
    Guid? ResponsibleUserId);

public sealed record CreateQualityActionRequest(
    Guid QualityCaseId,
    string Title,
    QualityActionType Type,
    Guid? AssignedUserId,
    DateTime? DueAtUtc);

public sealed record QualityCaseListItem(
    Guid Id,
    string Title,
    QualityCaseSource Source,
    string? SourceReference,
    Guid? MachineId,
    Guid? MaintenanceOrderId,
    Guid? InspectionId,
    QualityPriority Priority,
    QualityCaseStatus Status,
    string? RootCause,
    DateTime? DueAtUtc,
    Guid? ResponsibleUserId,
    int EscalationLevel,
    DateTime CreatedAtUtc);

public sealed record QualityActionListItem(
    Guid Id,
    Guid QualityCaseId,
    string Title,
    QualityActionType Type,
    QualityActionStatus Status,
    Guid? AssignedUserId,
    DateTime? DueAtUtc,
    DateTime CreatedAtUtc);

public sealed class CreateQualityCase
{
    private readonly IQualityCaseRepository _cases;

    public CreateQualityCase(IQualityCaseRepository cases)
    {
        _cases = cases;
    }

    public async Task<Guid> ExecuteAsync(CreateQualityCaseRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var qualityCase = new QualityCase(
            Guid.NewGuid(),
            request.Title,
            request.Description,
            request.Source,
            request.SourceReference,
            request.MachineId,
            request.MaintenanceOrderId,
            request.InspectionId,
            request.Priority,
            request.DueAtUtc,
            request.ResponsibleUserId);

        await _cases.AddAsync(qualityCase, cancellationToken);
        await _cases.SaveChangesAsync(cancellationToken);
        return qualityCase.Id;
    }
}

public sealed class CreateQualityAction
{
    private readonly IQualityCaseRepository _cases;
    private readonly IQualityActionRepository _actions;

    public CreateQualityAction(IQualityCaseRepository cases, IQualityActionRepository actions)
    {
        _cases = cases;
        _actions = actions;
    }

    public async Task<Guid> ExecuteAsync(CreateQualityActionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var qualityCase = await _cases.GetByIdAsync(request.QualityCaseId, cancellationToken)
            ?? throw new InvalidOperationException("Quality case was not found.");

        if (qualityCase.Status == QualityCaseStatus.Open)
        {
            qualityCase.StartRootCauseAnalysis("Action tracking started.");
        }

        var action = new QualityAction(
            Guid.NewGuid(),
            qualityCase.Id,
            request.Title,
            request.Type,
            request.AssignedUserId,
            request.DueAtUtc);

        await _actions.AddAsync(action, cancellationToken);
        await _actions.SaveChangesAsync(cancellationToken);
        await _cases.SaveChangesAsync(cancellationToken);
        return action.Id;
    }
}

public sealed class GetQualityCases
{
    private readonly IQualityCaseRepository _cases;

    public GetQualityCases(IQualityCaseRepository cases)
    {
        _cases = cases;
    }

    public Task<IReadOnlyList<QualityCaseListItem>> ExecuteAsync(CancellationToken cancellationToken = default)
        => _cases.GetAllAsync(cancellationToken);
}

public sealed class GetQualityActions
{
    private readonly IQualityActionRepository _actions;

    public GetQualityActions(IQualityActionRepository actions)
    {
        _actions = actions;
    }

    public Task<IReadOnlyList<QualityActionListItem>> ExecuteAsync(CancellationToken cancellationToken = default)
        => _actions.GetAllAsync(cancellationToken);
}
