using Microsoft.EntityFrameworkCore;
using SWFC.Application.M800_Security.M801_Access;
using SWFC.Domain.M800_Security.M801_Access;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M800_Security;

public sealed class AccessRuleReadRepository : IAccessRuleReadRepository
{
    private readonly AppDbContext _dbContext;

    public AccessRuleReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AccessRule>> GetByTargetsAsync(
        IReadOnlyCollection<AccessRuleTargetRef> targets,
        CancellationToken cancellationToken = default)
    {
        if (targets.Count == 0)
        {
            return Array.Empty<AccessRule>();
        }

        var targetIds = targets
            .Select(x => x.TargetId.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var targetTypes = targets
            .Select(x => x.TargetType)
            .Distinct()
            .ToArray();

        var roughMatches = await _dbContext.AccessRules
            .AsNoTracking()
            .Where(x => x.IsActive)
            .Where(x => targetTypes.Contains(x.TargetType))
            .Where(x => targetIds.Contains(x.TargetId))
            .ToListAsync(cancellationToken);

        var exactTargets = targets
            .Select(x => (x.TargetType, x.TargetId))
            .ToHashSet();

        return roughMatches
            .Where(x => exactTargets.Contains((x.TargetType, x.TargetId)))
            .ToList();
    }
}
