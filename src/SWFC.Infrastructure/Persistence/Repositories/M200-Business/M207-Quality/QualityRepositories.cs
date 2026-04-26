using Microsoft.EntityFrameworkCore;
using SWFC.Application.M200_Business.M207_Quality;
using SWFC.Domain.M200_Business.M207_Quality;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M200_Business.M207_Quality;

public sealed class QualityCaseRepository : IQualityCaseRepository
{
    private readonly AppDbContext _dbContext;

    public QualityCaseRepository(AppDbContext dbContext) => _dbContext = dbContext;

    public async Task AddAsync(QualityCase qualityCase, CancellationToken cancellationToken = default)
        => await _dbContext.Set<QualityCase>().AddAsync(qualityCase, cancellationToken);

    public Task<QualityCase?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _dbContext.Set<QualityCase>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<QualityCaseListItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var nowUtc = DateTime.UtcNow;
        return await _dbContext.Set<QualityCase>().AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new QualityCaseListItem(x.Id, x.Title, x.Source, x.SourceReference, x.MachineId, x.MaintenanceOrderId, x.InspectionId, x.Priority, x.Status, x.RootCause, x.DueAtUtc, x.ResponsibleUserId, x.DueAtUtc.HasValue && x.DueAtUtc.Value < nowUtc && x.Status != QualityCaseStatus.Resolved && x.Status != QualityCaseStatus.Closed ? (int)x.Priority : 0, x.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);
}

public sealed class QualityActionRepository : IQualityActionRepository
{
    private readonly AppDbContext _dbContext;

    public QualityActionRepository(AppDbContext dbContext) => _dbContext = dbContext;

    public async Task AddAsync(QualityAction action, CancellationToken cancellationToken = default)
        => await _dbContext.Set<QualityAction>().AddAsync(action, cancellationToken);

    public async Task<IReadOnlyList<QualityActionListItem>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Set<QualityAction>().AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new QualityActionListItem(x.Id, x.QualityCaseId, x.Title, x.Type, x.Status, x.AssignedUserId, x.DueAtUtc, x.CreatedAtUtc))
            .ToListAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);
}
