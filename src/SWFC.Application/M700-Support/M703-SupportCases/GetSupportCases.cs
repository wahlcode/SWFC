using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M700_Support.M703_SupportCases;

public sealed record GetSupportCasesQuery;

public sealed class GetSupportCasesPolicy : IAuthorizationPolicy<GetSupportCasesQuery>
{
    public AuthorizationRequirement GetRequirement(GetSupportCasesQuery request) =>
        new(requiredPermissions: new[] { "support.read" });
}

public sealed class GetSupportCasesHandler : IUseCaseHandler<GetSupportCasesQuery, IReadOnlyList<SupportCaseListItem>>
{
    private readonly ISupportCaseReadRepository _supportCaseReadRepository;

    public GetSupportCasesHandler(ISupportCaseReadRepository supportCaseReadRepository)
    {
        _supportCaseReadRepository = supportCaseReadRepository;
    }

    public async Task<Result<IReadOnlyList<SupportCaseListItem>>> HandleAsync(
        GetSupportCasesQuery command,
        CancellationToken cancellationToken = default)
    {
        var supportCases = await _supportCaseReadRepository.GetAllAsync(cancellationToken);

        var items = supportCases
            .OrderByDescending(x => x.AuditInfo.LastModifiedAtUtc ?? x.AuditInfo.CreatedAtUtc)
            .Select(x => new SupportCaseListItem(
                x.Id,
                x.UserRequest,
                x.ProblemDescription,
                x.TriggeredBugId,
                x.TriggeredIncidentId,
                x.AuditInfo.CreatedAtUtc,
                x.AuditInfo.CreatedBy,
                x.AuditInfo.LastModifiedAtUtc,
                x.AuditInfo.LastModifiedBy))
            .ToList();

        return Result<IReadOnlyList<SupportCaseListItem>>.Success(items);
    }
}
