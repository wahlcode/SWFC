using Microsoft.EntityFrameworkCore;
using SWFC.Application.M800_Security.M801_Access;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M800_Security.M801_Access;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M800_Security;

public sealed class AccessRuleWriteRepository : IAccessRuleWriteRepository
{
    private readonly AppDbContext _dbContext;

    public AccessRuleWriteRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(AccessRule accessRule, CancellationToken cancellationToken = default)
    {
        return _dbContext.AccessRules.AddAsync(accessRule, cancellationToken).AsTask();
    }

    public Task<AccessRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.AccessRules
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public void Deactivate(AccessRule accessRule, ChangeContext changeContext)
    {
        accessRule.Deactivate(changeContext);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
