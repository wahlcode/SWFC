using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M700_Support.M705_Knowledge_Base;

public sealed record GetKnowledgeEntriesQuery;

public sealed class GetKnowledgeEntriesPolicy : IAuthorizationPolicy<GetKnowledgeEntriesQuery>
{
    public AuthorizationRequirement GetRequirement(GetKnowledgeEntriesQuery request) =>
        new(requiredPermissions: new[] { "support.read" });
}

public sealed class GetKnowledgeEntriesHandler : IUseCaseHandler<GetKnowledgeEntriesQuery, IReadOnlyList<KnowledgeEntryListItem>>
{
    private readonly IKnowledgeEntryReadRepository _knowledgeEntryReadRepository;

    public GetKnowledgeEntriesHandler(IKnowledgeEntryReadRepository knowledgeEntryReadRepository)
    {
        _knowledgeEntryReadRepository = knowledgeEntryReadRepository;
    }

    public async Task<Result<IReadOnlyList<KnowledgeEntryListItem>>> HandleAsync(
        GetKnowledgeEntriesQuery command,
        CancellationToken cancellationToken = default)
    {
        var knowledgeEntries = await _knowledgeEntryReadRepository.GetAllAsync(cancellationToken);

        var items = knowledgeEntries
            .OrderByDescending(x => x.AuditInfo.LastModifiedAtUtc ?? x.AuditInfo.CreatedAtUtc)
            .Select(x => new KnowledgeEntryListItem(
                x.Id,
                x.Type,
                x.Content,
                x.AuditInfo.CreatedAtUtc,
                x.AuditInfo.CreatedBy,
                x.AuditInfo.LastModifiedAtUtc,
                x.AuditInfo.LastModifiedBy))
            .ToList();

        return Result<IReadOnlyList<KnowledgeEntryListItem>>.Success(items);
    }
}
