using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M800_Security.M805_AuditCompliance.Entries;

public sealed record GetAuditEntriesQuery(int Take = 200);

public sealed class GetAuditEntriesPolicy : IAuthorizationPolicy<GetAuditEntriesQuery>
{
    public AuthorizationRequirement GetRequirement(GetAuditEntriesQuery request) =>
        new(requiredPermissions: new[] { "security.read" });
}

public sealed class GetAuditEntriesHandler : IUseCaseHandler<GetAuditEntriesQuery, IReadOnlyList<AuditEntryListItem>>
{
    private readonly IAuditLogRepository _auditLogRepository;

    public GetAuditEntriesHandler(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task<Result<IReadOnlyList<AuditEntryListItem>>> HandleAsync(
        GetAuditEntriesQuery request,
        CancellationToken cancellationToken = default)
    {
        var take = request.Take switch
        {
            <= 0 => 200,
            > 500 => 500,
            _ => request.Take
        };

        var entries = await _auditLogRepository.GetRecentAsync(take, cancellationToken);

        var result = entries
            .Select(x => new AuditEntryListItem(
                x.Id,
                x.TimestampUtc,
                x.ActorUserId,
                x.ActorDisplayName,
                x.TargetUserId,
                x.Action,
                x.Module,
                x.ObjectType,
                x.ObjectId,
                x.OldValues,
                x.NewValues,
                x.ClientIp,
                x.ClientInfo,
                x.ApprovedByUserId,
                x.Reason))
            .ToList();

        return Result<IReadOnlyList<AuditEntryListItem>>.Success(result);
    }
}
