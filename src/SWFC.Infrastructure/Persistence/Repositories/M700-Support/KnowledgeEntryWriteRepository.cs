using Microsoft.EntityFrameworkCore;
using SWFC.Application.M700_Support.M705_Knowledge_Base;
using SWFC.Domain.M700_Support.M705_Knowledge_Base;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M700_Support;

public sealed class KnowledgeEntryWriteRepository : IKnowledgeEntryWriteRepository
{
    private readonly AppDbContext _dbContext;

    public KnowledgeEntryWriteRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(KnowledgeEntry knowledgeEntry, CancellationToken cancellationToken = default)
    {
        await _dbContext.KnowledgeEntries.AddAsync(knowledgeEntry, cancellationToken);
    }

    public Task<KnowledgeEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.KnowledgeEntries.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
