using Microsoft.EntityFrameworkCore;
using SWFC.Application.M200_Business.M203_Inspections;
using SWFC.Domain.M200_Business.M203_Inspections;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M200_Business.M203_Inspections;

public sealed class InspectionPlanRepository : IInspectionPlanRepository
{
    private readonly AppDbContext _dbContext;

    public InspectionPlanRepository(AppDbContext dbContext) => _dbContext = dbContext;

    public async Task AddAsync(InspectionPlan plan, CancellationToken cancellationToken = default)
        => await _dbContext.Set<InspectionPlan>().AddAsync(plan, cancellationToken);

    public Task<InspectionPlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _dbContext.Set<InspectionPlan>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<InspectionPlanListItem>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Set<InspectionPlan>().AsNoTracking()
            .OrderBy(x => x.NextDueAtUtc)
            .Select(x => new InspectionPlanListItem(x.Id, x.Name, x.TargetType, x.TargetId, x.ObjectType, x.IntervalDays, x.NextDueAtUtc, x.ResponsibleUserId, x.DocumentReference, x.IsActive))
            .ToListAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);
}

public sealed class InspectionRepository : IInspectionRepository
{
    private readonly AppDbContext _dbContext;

    public InspectionRepository(AppDbContext dbContext) => _dbContext = dbContext;

    public async Task AddAsync(Inspection inspection, CancellationToken cancellationToken = default)
        => await _dbContext.Set<Inspection>().AddAsync(inspection, cancellationToken);

    public async Task<IReadOnlyList<InspectionListItem>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Set<Inspection>().AsNoTracking()
            .OrderByDescending(x => x.PerformedAtUtc)
            .Select(x => new InspectionListItem(x.Id, x.InspectionPlanId, x.TargetType, x.TargetId, x.Title, x.Result, x.Status, x.InspectorUserId, x.Notes, x.FollowUpReference, x.PerformedAtUtc))
            .ToListAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);
}
