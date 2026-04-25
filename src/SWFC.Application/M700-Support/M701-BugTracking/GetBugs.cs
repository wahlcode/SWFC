using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M700_Support.M701_BugTracking;

public sealed record GetBugsQuery;

public sealed class GetBugsPolicy : IAuthorizationPolicy<GetBugsQuery>
{
    public AuthorizationRequirement GetRequirement(GetBugsQuery request) =>
        new(requiredPermissions: new[] { "support.read" });
}

public sealed class GetBugsHandler : IUseCaseHandler<GetBugsQuery, IReadOnlyList<BugListItem>>
{
    private readonly IBugReadRepository _bugReadRepository;

    public GetBugsHandler(IBugReadRepository bugReadRepository)
    {
        _bugReadRepository = bugReadRepository;
    }

    public async Task<Result<IReadOnlyList<BugListItem>>> HandleAsync(
        GetBugsQuery command,
        CancellationToken cancellationToken = default)
    {
        var bugs = await _bugReadRepository.GetAllAsync(cancellationToken);

        var items = bugs
            .OrderByDescending(x => x.AuditInfo.LastModifiedAtUtc ?? x.AuditInfo.CreatedAtUtc)
            .Select(x => new BugListItem(
                x.Id,
                x.Description,
                x.Reproducibility,
                x.Status,
                x.AuditInfo.CreatedAtUtc,
                x.AuditInfo.CreatedBy,
                x.AuditInfo.LastModifiedAtUtc,
                x.AuditInfo.LastModifiedBy))
            .ToList();

        return Result<IReadOnlyList<BugListItem>>.Success(items);
    }
}
