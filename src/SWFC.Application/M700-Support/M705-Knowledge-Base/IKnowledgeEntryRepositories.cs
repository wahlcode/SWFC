using SWFC.Domain.M700_Support.M705_Knowledge_Base;

namespace SWFC.Application.M700_Support.M705_Knowledge_Base;

public interface IKnowledgeEntryReadRepository
{
    Task<IReadOnlyList<KnowledgeEntry>> GetAllAsync(CancellationToken cancellationToken = default);
}

public interface IKnowledgeEntryWriteRepository
{
    Task AddAsync(KnowledgeEntry knowledgeEntry, CancellationToken cancellationToken = default);
    Task<KnowledgeEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
