using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M700_Support.M702_ChangeRequests;

public sealed record GetChangeRequestsQuery;

public sealed class GetChangeRequestsPolicy : IAuthorizationPolicy<GetChangeRequestsQuery>
{
    public AuthorizationRequirement GetRequirement(GetChangeRequestsQuery request) =>
        new(requiredPermissions: new[] { "support.read" });
}

public sealed class GetChangeRequestsHandler : IUseCaseHandler<GetChangeRequestsQuery, IReadOnlyList<ChangeRequestListItem>>
{
    private readonly IChangeRequestReadRepository _changeRequestReadRepository;

    public GetChangeRequestsHandler(IChangeRequestReadRepository changeRequestReadRepository)
    {
        _changeRequestReadRepository = changeRequestReadRepository;
    }

    public async Task<Result<IReadOnlyList<ChangeRequestListItem>>> HandleAsync(
        GetChangeRequestsQuery command,
        CancellationToken cancellationToken = default)
    {
        var changeRequests = await _changeRequestReadRepository.GetAllAsync(cancellationToken);

        var items = changeRequests
            .OrderByDescending(x => x.AuditInfo.LastModifiedAtUtc ?? x.AuditInfo.CreatedAtUtc)
            .Select(x => new ChangeRequestListItem(
                x.Id,
                x.Type,
                x.Description,
                x.RequirementReference,
                x.RoadmapReference,
                x.AuditInfo.CreatedAtUtc,
                x.AuditInfo.CreatedBy,
                x.AuditInfo.LastModifiedAtUtc,
                x.AuditInfo.LastModifiedBy))
            .ToList();

        return Result<IReadOnlyList<ChangeRequestListItem>>.Success(items);
    }
}
