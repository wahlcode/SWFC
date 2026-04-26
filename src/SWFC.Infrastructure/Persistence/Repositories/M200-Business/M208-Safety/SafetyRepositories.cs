using Microsoft.EntityFrameworkCore;
using SWFC.Application.M200_Business.M208_Safety;
using SWFC.Domain.M200_Business.M208_Safety;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M200_Business.M208_Safety;

public sealed class SafetyAssessmentRepository : ISafetyAssessmentRepository
{
    private readonly AppDbContext _dbContext;

    public SafetyAssessmentRepository(AppDbContext dbContext) => _dbContext = dbContext;

    public async Task AddAsync(SafetyAssessment assessment, CancellationToken cancellationToken = default)
        => await _dbContext.Set<SafetyAssessment>().AddAsync(assessment, cancellationToken);

    public Task<SafetyAssessment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _dbContext.Set<SafetyAssessment>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<SafetyAssessmentListItem>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Set<SafetyAssessment>().AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new SafetyAssessmentListItem(x.Id, x.Activity, x.TargetType, x.TargetId, x.Hazard, x.Likelihood, x.Severity, x.Likelihood * x.Severity, x.RequiredMeasures, x.ResponsibleUserId, x.DocumentReference, x.QualityCaseId, x.Status, x.CreatedAtUtc))
            .ToListAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);
}

public sealed class SafetyPermitRepository : ISafetyPermitRepository
{
    private readonly AppDbContext _dbContext;

    public SafetyPermitRepository(AppDbContext dbContext) => _dbContext = dbContext;

    public async Task AddAsync(SafetyPermit permit, CancellationToken cancellationToken = default)
        => await _dbContext.Set<SafetyPermit>().AddAsync(permit, cancellationToken);

    public async Task<IReadOnlyList<SafetyPermitListItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var nowUtc = DateTime.UtcNow;
        return await _dbContext.Set<SafetyPermit>().AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new SafetyPermitListItem(x.Id, x.AssessmentId, x.Activity, x.ValidUntilUtc, x.Restriction, x.ApprovedByUserId, x.Status, x.Status == SafetyPermitStatus.Approved && x.ValidUntilUtc >= nowUtc, x.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);
}
