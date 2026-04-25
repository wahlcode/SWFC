using Microsoft.EntityFrameworkCore;
using SWFC.Application.M700_Support.M705_Knowledge_Base;
using SWFC.Domain.M700_Support.M705_Knowledge_Base;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M700_Support;

public sealed class KnowledgeEntryReadRepository : IKnowledgeEntryReadRepository
{
    private readonly AppDbContext _dbContext;

    public KnowledgeEntryReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<KnowledgeEntry>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.KnowledgeEntries
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
