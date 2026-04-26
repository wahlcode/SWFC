using SWFC.Domain.M200_Business.M208_Safety;

namespace SWFC.Application.M200_Business.M208_Safety;

public interface ISafetyAssessmentRepository
{
    Task AddAsync(SafetyAssessment assessment, CancellationToken cancellationToken = default);
    Task<SafetyAssessment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SafetyAssessmentListItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface ISafetyPermitRepository
{
    Task AddAsync(SafetyPermit permit, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SafetyPermitListItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public sealed record CreateSafetyAssessmentRequest(
    string Activity,
    SafetyTargetType TargetType,
    Guid TargetId,
    string Hazard,
    int Likelihood,
    int Severity,
    string RequiredMeasures,
    Guid? ResponsibleUserId,
    string? DocumentReference,
    Guid? QualityCaseId);

public sealed record CreateSafetyPermitRequest(
    Guid AssessmentId,
    string Activity,
    DateTime ValidUntilUtc,
    string? Restriction,
    Guid? ApprovedByUserId);

public sealed record SafetyAssessmentListItem(
    Guid Id,
    string Activity,
    SafetyTargetType TargetType,
    Guid TargetId,
    string Hazard,
    int Likelihood,
    int Severity,
    int RiskScore,
    string RequiredMeasures,
    Guid? ResponsibleUserId,
    string? DocumentReference,
    Guid? QualityCaseId,
    SafetyAssessmentStatus Status,
    DateTime CreatedAtUtc);

public sealed record SafetyPermitListItem(
    Guid Id,
    Guid AssessmentId,
    string Activity,
    DateTime ValidUntilUtc,
    string Restriction,
    Guid? ApprovedByUserId,
    SafetyPermitStatus Status,
    bool AllowsAction,
    DateTime CreatedAtUtc);

public sealed class CreateSafetyAssessment
{
    private readonly ISafetyAssessmentRepository _assessments;

    public CreateSafetyAssessment(ISafetyAssessmentRepository assessments)
    {
        _assessments = assessments;
    }

    public async Task<Guid> ExecuteAsync(CreateSafetyAssessmentRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var assessment = new SafetyAssessment(
            Guid.NewGuid(),
            request.Activity,
            request.TargetType,
            request.TargetId,
            request.Hazard,
            request.Likelihood,
            request.Severity,
            request.RequiredMeasures,
            request.ResponsibleUserId,
            request.DocumentReference,
            request.QualityCaseId);

        await _assessments.AddAsync(assessment, cancellationToken);
        await _assessments.SaveChangesAsync(cancellationToken);
        return assessment.Id;
    }
}

public sealed class CreateSafetyPermit
{
    private readonly ISafetyAssessmentRepository _assessments;
    private readonly ISafetyPermitRepository _permits;

    public CreateSafetyPermit(ISafetyAssessmentRepository assessments, ISafetyPermitRepository permits)
    {
        _assessments = assessments;
        _permits = permits;
    }

    public async Task<Guid> ExecuteAsync(CreateSafetyPermitRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var assessment = await _assessments.GetByIdAsync(request.AssessmentId, cancellationToken)
            ?? throw new InvalidOperationException("Safety assessment was not found.");

        var permit = new SafetyPermit(
            Guid.NewGuid(),
            assessment.Id,
            request.Activity,
            request.ValidUntilUtc,
            request.Restriction,
            request.ApprovedByUserId);

        await _permits.AddAsync(permit, cancellationToken);
        await _permits.SaveChangesAsync(cancellationToken);
        return permit.Id;
    }
}

public sealed class GetSafetyAssessments
{
    private readonly ISafetyAssessmentRepository _assessments;

    public GetSafetyAssessments(ISafetyAssessmentRepository assessments)
    {
        _assessments = assessments;
    }

    public Task<IReadOnlyList<SafetyAssessmentListItem>> ExecuteAsync(CancellationToken cancellationToken = default)
        => _assessments.GetAllAsync(cancellationToken);
}

public sealed class GetSafetyPermits
{
    private readonly ISafetyPermitRepository _permits;

    public GetSafetyPermits(ISafetyPermitRepository permits)
    {
        _permits = permits;
    }

    public Task<IReadOnlyList<SafetyPermitListItem>> ExecuteAsync(CancellationToken cancellationToken = default)
        => _permits.GetAllAsync(cancellationToken);
}
